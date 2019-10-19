using Jerrycurl.Mvc.Projections;
using System;

namespace Jerrycurl.Mvc
{
    internal class ProcRenderer : IProcRenderer
    {
        public ProcContext Context { get; }

        public ProcRenderer(ProcContext context)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ISqlContent Body()
        {
            if (this.Context.Stack.IsEmpty)
                throw new ProcExecutionException("Execution stack is empty. Please only call Body() on template pages.");

            this.Context.Executing.Body?.Invoke();

            return SqlContent.Empty;
        }

        public ISqlContent Partial(string procName, IProjection model, IProjection result)
        {
            if (procName == null)
                throw new ArgumentNullException(nameof(procName));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            PageDescriptor descriptor = this.Context.Locator.FindPage(procName, model.Context.Executing.Page.PageType);
            PageFactory factory = this.Context.Domain.Engine.Page(descriptor.PageType);

            this.Context.Stack.Push(new PageExecutionContext() { Page = descriptor });

            factory(model, result);

            this.Context.Stack.Pop();

            return SqlContent.Empty;
        }
    }
}
