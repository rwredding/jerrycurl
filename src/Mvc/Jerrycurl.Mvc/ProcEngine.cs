using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Reflection;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using System.Linq.Expressions;
using System.Linq;
using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Reflection;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Sql;

namespace Jerrycurl.Mvc
{
    public class ProcEngine : IProcEngine
    {
        private delegate ISqlPage PageConstructor(IProjection model, IProjection result);

        public ProcFactory Proc(PageDescriptor descriptor, ProcArgs args)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (descriptor.DomainType == null)
                throw new ProcExecutionException($"No domain found for page type '{descriptor.PageType.GetSanitizedFullName()}'. Make sure to implement IDomain in a parent namespace.");

            ProcCacheKey key = new ProcCacheKey(descriptor, args);

            return ProcCache.Procs.GetOrAdd(key, _ => this.CreateProcFactory(descriptor, args));
        }

        public PageFactory Page(Type pageType)
        {
            if (pageType == null)
                throw new ArgumentNullException(nameof(pageType));

            return ProcCache.Pages.GetOrAdd(pageType, _ => this.CreatePageFactory(pageType));
        }

        public IDomainOptions Options(Type domainType)
        {
            if (domainType == null)
                throw new ArgumentNullException(nameof(domainType));

            return ProcCache.Domains.GetOrAdd(domainType, _ => new Lazy<IDomainOptions>(() => this.GetConfiguredOptions(domainType))).Value;
        }

        private PageFactory CreatePageFactory(Type pageType)
        {
            Type resultType = this.GetResultTypeFromPage(pageType);
            Type modelType = this.GetModelTypeFromPage(pageType);

            if (resultType == null || modelType == null)
                throw new InvalidOperationException("Type must inherit ProcPage<TModel, TResult>.");

            PageConstructor constructor = this.CreatePageConstructor(pageType);

            void pageFactory(IProjection model, IProjection result)
            {
                IProjection newModel = this.GetProjectionForPartialModel(model, modelType);
                IProjection newResult = this.GetProjectionForPartialResult(result, resultType);

                ISqlPage page = constructor(newModel, newResult);

                page.Execute();
            }

            return pageFactory;
        }

        private IProjection GetProjectionForPartialModel(IProjection projection, Type modelType)
        {
            if (modelType.IsAssignableFrom(projection.Metadata.Type))
                return projection;
            else if (projection.Metadata.Item != null && modelType.IsAssignableFrom(projection.Metadata.Item.Type))
                return projection.With(metadata: projection.Metadata.Item);
            else
                throw new InvalidOperationException($"No conversion found between page type '{modelType.GetSanitizedFullName()}' and requested type '{projection.Metadata.Type.GetSanitizedFullName()}'.");
        }

        private IProjection GetProjectionForPartialResult(IProjection projection, Type resultType)
        {
            if (resultType.IsAssignableFrom(projection.Metadata.Type))
                return projection;
            else if (projection.Metadata.Item != null && resultType.IsAssignableFrom(projection.Metadata.Item.Type))
                return projection.With(metadata: projection.Metadata.Item);
            else
                throw new InvalidOperationException($"No conversion found between page type '{resultType.GetSanitizedFullName()}' and requested type '{projection.Metadata.Type.GetSanitizedFullName()}'.");
        }

        private IEnumerable<PageDescriptor> GetTemplateHierarchy(PageDescriptor descriptor)
        {
            yield return descriptor;

            PageDescriptor template = this.GetTemplateDescriptor(descriptor);

            if (template != null)
            {
                foreach (PageDescriptor template2 in this.GetTemplateHierarchy(template))
                    yield return template2;
            }
        }

        private PageDescriptor GetTemplateDescriptor(PageDescriptor descriptor)
        {
            if (this.HasTemplateAnnotation(descriptor.PageType, out string procName))
                return descriptor.Locator.FindPage(procName, descriptor.PageType);

            return null;
        }

        private bool HasTemplateAnnotation(Type pageType, out string templateName)
        {
            TemplateAttribute annotation = pageType.GetCustomAttribute<TemplateAttribute>();

            templateName = annotation?.ProcName;

            return !string.IsNullOrWhiteSpace(templateName);
        }

        private ProcFactory CreateProcFactory(PageDescriptor descriptor, ProcArgs args)
        {
            IDomainOptions options = this.Options(descriptor.DomainType);

            PageDescriptor[] template = this.GetTemplateHierarchy(descriptor).Reverse().ToArray();
            PageConstructor[] factories = template.Select(t => this.CreatePageConstructor(t.PageType)).Concat(new PageConstructor[1]).ToArray();

            IProjectionMetadata resultMetadata = this.GetMetadataForResult(descriptor, args, options);
            IProjectionMetadata modelMetadata = this.GetMetadataForModel(descriptor, args, options);

            ISchema resultSchema = resultMetadata.Identity.Schema;
            ISchema modelSchema = modelMetadata.Identity.Schema;

            IProcLocator locator = descriptor.Locator;

            Type originType = descriptor.OriginType;

            IProcResult procFactory(object model)
            {
                ProcContext context = this.CreateContext(descriptor);

                IProjectionIdentity modelIdentity = new ProjectionIdentity(modelSchema, new Relation(model, modelSchema));
                IProjectionIdentity resultIdentity = new ProjectionIdentity(resultSchema);

                IProjection modelProjection = new Projection(modelIdentity, context, modelMetadata);
                IProjection resultProjection = new Projection(resultIdentity, context, resultMetadata);

                PageExecutionContext[] execution = template.Select(t => new PageExecutionContext() { Page = t }).ToArray();

                execution[0].Buffer = new ProcBuffer();

                for (int i = 0; i < execution.Length; i++)
                {
                    PageConstructor nextFactory = factories[i + 1];

                    if (nextFactory != null)
                    {
                        PageExecutionContext nextContext = execution[i + 1];

                        execution[i].Body = () =>
                        {
                            ISqlPage bodyPage = nextFactory(modelProjection, resultProjection);

                            context.Stack.Push(nextContext);

                            bodyPage.Execute();

                            context.Stack.Pop();
                        };
                    }
                }

                ISqlPage page = factories[0](modelProjection, resultProjection);

                context.Stack.Push(execution[0]);

                page.Execute();

                return new ProcResult()
                {
                    Buffer = context.Executing.Buffer,
                    Domain = options,
                };
            }

            return procFactory;
        }

        private IProjectionMetadata GetMetadataForResult(PageDescriptor descriptor, ProcArgs args, IDomainOptions options)
        {
            Type itemType = this.GetResultTypeFromPage(descriptor.PageType);
            Type pageType = typeof(IList<>).MakeGenericType(itemType);
            Type argsType = args.ResultType ?? typeof(IReadOnlyList<object>);

            ISchema pageSchema = options.Schemas.GetSchema(pageType);
            ISchema argsSchema = options.Schemas.GetSchema(argsType);

            IProjectionMetadata pageMetadata = pageSchema.GetMetadata<IProjectionMetadata>() ?? throw new InvalidOperationException($"Metadata not found for type '{pageType.Name}'");
            IProjectionMetadata argsMetadata = argsSchema.GetMetadata<IProjectionMetadata>() ?? throw new InvalidOperationException($"Metadata not found for type '{argsType.Name}'");

            if (pageMetadata.Item != null && argsMetadata.Item != null && pageMetadata.Item.Type.IsAssignableFrom(argsMetadata.Item.Type))
                return argsMetadata.Item;
            else if (pageMetadata.Item != null && argsMetadata.Item != null && argsMetadata.Item.Type.IsAssignableFrom(pageMetadata.Item.Type))
                return pageMetadata.Item;
            else if (pageMetadata.Item != null && pageMetadata.Item.Type.IsAssignableFrom(argsMetadata.Type))
                return argsMetadata;
            else if (argsMetadata.Item != null && argsMetadata.Item.Type.IsAssignableFrom(pageMetadata.Type))
                return pageMetadata;
            else if (argsMetadata.Type.IsAssignableFrom(pageMetadata.Type))
                return pageMetadata;
            else if (pageMetadata.Type.IsAssignableFrom(argsMetadata.Type))
                return argsMetadata;
            else
                throw new InvalidOperationException($"No conversion found between page type '{pageMetadata.Type.GetSanitizedFullName()}' and requested type '{argsMetadata.Type.GetSanitizedFullName()}'.");
        }

        private IProjectionMetadata GetMetadataForModel(PageDescriptor descriptor, ProcArgs args, IDomainOptions options)
        {
            Type pageType = this.GetModelTypeFromPage(descriptor.PageType);
            Type argsType = args.ModelType ?? typeof(object);

            ISchema pageSchema = options.Schemas.GetSchema(pageType);
            ISchema argsSchema = options.Schemas.GetSchema(argsType);

            IProjectionMetadata pageMetadata = pageSchema.GetMetadata<IProjectionMetadata>() ?? throw new InvalidOperationException($"Metadata not found for type '{pageType.Name}'");
            IProjectionMetadata argsMetadata = argsSchema.GetMetadata<IProjectionMetadata>() ?? throw new InvalidOperationException($"Metadata not found for type '{argsType.Name}'");

            if (argsMetadata.Item != null && pageMetadata.Type.IsAssignableFrom(argsMetadata.Item.Type))
                return argsMetadata.Item;
            else if (pageMetadata.Type.IsAssignableFrom(argsMetadata.Type))
                return argsMetadata;
            else if (argsMetadata.Type.IsAssignableFrom(pageMetadata.Type))
                return pageMetadata;
            else
                throw new InvalidOperationException($"No conversion found between page type '{pageMetadata.Type.FullName}' and requested type '{argsMetadata.Type.FullName}'.");
        }

        private Type GetTypeArgumentFromPage(Type pageType, int argumentIndex)
        {
            while ((pageType = pageType.BaseType) != null)
            {
                if (pageType.IsOpenGeneric(typeof(ProcPage<,>), out Type[] args))
                    return args[argumentIndex];
            }

            return null;
        }

        private Type GetModelTypeFromPage(Type pageType) => this.GetTypeArgumentFromPage(pageType, 0);
        private Type GetResultTypeFromPage(Type pageType) => this.GetTypeArgumentFromPage(pageType, 1);

        private IEnumerable<PropertyInfo> GetInjectionProperties(Type pageType) => pageType.GetProperties().Where(t => t.GetCustomAttribute<InjectAttribute>() != null);

        private PageConstructor CreatePageConstructor(Type pageType)
        {
            ConstructorInfo ctor = pageType.GetConstructor(new[] { typeof(IProjection), typeof(IProjection) });

            if (ctor == null)
                throw new InvalidOperationException("Constructor not found for specified signature.");

            ParameterExpression model = Expression.Parameter(typeof(IProjection), "model");
            ParameterExpression result = Expression.Parameter(typeof(IProjection), "result");

            NewExpression newPage = Expression.New(ctor, model, result);

            IEnumerable<PropertyInfo> injections = this.GetInjectionProperties(pageType);

            if (injections.Any())
            {
                ParameterExpression contextVar = Expression.Variable(typeof(IProcContext), "context");
                ParameterExpression servicesVar = Expression.Variable(typeof(IServiceResolver), "services");

                Expression context = Expression.Property(model, "Context");
                Expression domain = Expression.Property(context, nameof(IProcContext.Domain));
                Expression services = Expression.Property(domain, nameof(IDomainOptions.Services));

                Expression assignContext = Expression.Assign(contextVar, context);
                Expression assignServices = Expression.Assign(servicesVar, services);

                MemberInitExpression pageInit = Expression.MemberInit(newPage, injections.Select(p => this.GetInjectionBinding(p, contextVar, servicesVar)));

                Expression block = Expression.Block(new[] { contextVar, servicesVar }, assignContext, assignServices, pageInit);

                return Expression.Lambda<PageConstructor>(block, model, result).Compile();
            }

            return Expression.Lambda<PageConstructor>(newPage, model, result).Compile();
        }

        private MemberBinding GetInjectionBinding(PropertyInfo property, Expression context, Expression services)
        {
            MethodInfo injectMethod = typeof(IServiceResolver).GetMethod(nameof(IServiceResolver.GetService));
            MethodInfo projectMethod = typeof(IServiceResolver).GetMethod(nameof(IServiceResolver.GetProjection));

            if (property.PropertyType.IsOpenGeneric(typeof(IProjection<>), out Type itemType))
            {
                MethodInfo createMethod = projectMethod.MakeGenericMethod(itemType);
                Expression createCall = Expression.Call(services, createMethod, context);

                return Expression.Bind(property, createCall);
            }
            else
            {
                MethodInfo createMethod = injectMethod.MakeGenericMethod(property.PropertyType);
                Expression createCall = Expression.Call(services, createMethod);

                return Expression.Bind(property, createCall);
            }
        }

        private IDomainOptions GetConfiguredOptions(Type domainType)
        {
            IDomain domain = this.CreateDomain(domainType);

            DomainOptions options = this.GetDefaultOptions();

            domain.Configure(options);

            return options;
        }

        private IDomain CreateDomain(Type domainType)
        {
            try
            {
                return (IDomain)Activator.CreateInstance(domainType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to create domain.", ex);
            }
        }

        private ProcContext CreateContext(PageDescriptor descriptor)
        {
            IDomainOptions options = this.Options(descriptor.DomainType);

            return new ProcContext(descriptor, options);
        }

        protected virtual DomainOptions GetDefaultOptions()
        {
            return new DomainOptions()
            {
                Dialect = new IsoDialect(),
                Schemas = new SchemaStore(new DotNotation())
                {
                    new RelationMetadataBuilder(),
                    new BindingMetadataBuilder(),
                    new ReferenceMetadataBuilder(),
                    new TableMetadataBuilder(),
                    new ProjectionMetadataBuilder(),
                    new JsonMetadataBuilder(),
                },
                Engine = this,
                ConnectionFactory = () => throw new InvalidOperationException("No connection factory specified."),
                Services = new ServiceResolver(),
                Sql = new SqlOptions(),
            };
        }
    }
}
