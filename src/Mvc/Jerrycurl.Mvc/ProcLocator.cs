using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Jerrycurl.CodeAnalysis;

namespace Jerrycurl.Mvc
{
    public class ProcLocator : IProcLocator
    {
        public PageDescriptor FindPage(string procName, Type originType)
        {
            if (string.IsNullOrWhiteSpace(procName))
                throw new ArgumentException("Argument cannot be empty.", nameof(procName));

            if (originType == null)
                throw new ArgumentNullException(nameof(originType));

            PageLocatorKey key = new PageLocatorKey(procName, originType);

            return ProcCache.PageDescriptors.GetOrAdd(key, _ => this.CreateDescriptor(procName, originType));
        }

        public DomainDescriptor FindDomain(Type originType)
        {
            if (originType == null)
                throw new ArgumentNullException(nameof(originType));

            Type domainType = this.GetDomainType(originType);

            if (domainType == null)
                return null;

            return ProcCache.DomainDescriptors.GetOrAdd(originType, _ => new DomainDescriptor()
            {
                DomainType = domainType,
                OriginType = originType,
            });
        }

        private PageDescriptor CreateDescriptor(string procName, Type originType)
        {
            Type domainType = this.GetDomainType(originType);

            Namespace forPath = this.GetPathNamespace(procName, originType, domainType);

            IEnumerable<Namespace> typeNames;

            if (forPath != null)
                typeNames = new[] { forPath.Up().Add(this.SanitizeTypeName(forPath.AsTypeName().Name)) };
            else
            {
                typeNames = this.GetPageNamespacesToSearch(originType).Distinct().Select(ns => ns.Add(Namespace.FromPath(procName)));

                typeNames = typeNames.Select(ns => ns.Up().Add(this.SanitizeTypeName(ns.AsTypeName().Name)));
            }

            foreach (string typeName in typeNames.Select(ns => ns.Definition))
            {
                Type pageType = originType.Assembly.GetType(typeName, throwOnError: false, ignoreCase: true);

                if (typeof(ISqlPage).IsAssignableFrom(pageType))
                {
                    return new PageDescriptor()
                    {
                        OriginType = originType,
                        PageType = pageType,
                        DomainType = domainType,
                        Locator = this,
                    };
                }
            }

            throw this.GetPageNotFoundException(procName, typeNames);
        }

        private PageNotFoundException GetPageNotFoundException(string procName, IEnumerable<Namespace> typeNames)
        {
            string lookedIn = string.Join(", ", typeNames.Select(ns => ns.Up().Definition ?? "<global>"));

            return new PageNotFoundException($"Page not found from procedure name '{procName}'. Looked in: {lookedIn}.");
        }

        private Type GetDomainType(Type originType)
        {
            if (this.IsDomainType(originType))
                return originType;

            Type[] allTypes = originType.Assembly.DefinedTypes.Select(ti => ti.AsType()).ToArray();

            foreach (Namespace ns in Namespace.FromType(originType).Traverse())
            {
                Type[] domainTypes = allTypes.Where(t => t.Namespace == ns.Definition && this.IsDomainType(t)).ToArray();

                if (domainTypes.Length == 1)
                    return domainTypes[0];
                else if (domainTypes.Length > 1)
                    throw new InvalidOperationException($"Multiple domains found in namespace '{ns}'.");
            }

            return null;
        }

        private Namespace GetPathNamespace(string procName, Type originType, Type domainType)
        {
            if (this.IsDomainPath(procName, out string path))
            {
                if (domainType == null)
                    return Namespace.FromPath(path);

                string domainPath = Namespace.FromType(domainType).ToPath();

                return Namespace.FromPath(Path.Combine(domainPath, path));
            }
            else if (this.IsAbsolutePath(procName, out path))
                return Namespace.FromPath(path);
            else if (this.IsRelativePath(procName, out _))
            {
                string originPath = Namespace.FromType(originType).ToPath();

                return Namespace.FromPath(Path.Combine(originPath, procName));
            }

            return null;
        }

        private IEnumerable<Namespace> GetPageNamespacesToSearch(Type originType)
        {
            Namespace ns = Namespace.FromType(originType);

            if (this.IsAccessorType(originType))
            {
                if (ns.AsTypeName().Name == "Accessors")
                    ns = ns.Up();

                if (this.HasAccessorSuffix(originType, out string prefix))
                {
                    yield return ns.Add("Queries").Add(prefix);
                    yield return ns.Add("Commands").Add(prefix);

                    yield return ns.Add("Queries.Shared");
                    yield return ns.Add("Commands.Shared");
                }

                yield return ns.Add("Queries");
                yield return ns.Add("Commands");
            }
            else if (this.IsPageType(originType))
            {
                Namespace root = ns.Traverse().FirstOrDefault(ns2 => ns2.AsTypeName().Name == "Queries" || ns2.AsTypeName().Name == "Commands");

                yield return ns;

                if (root != null)
                {
                    yield return root;
                    yield return root.Add("Shared");
                }
            }
        }

        private bool IsDomainType(Type type) => typeof(IDomain).IsAssignableFrom(type);
        private bool IsAccessorType(Type type) => (typeof(Accessor).IsAssignableFrom(type) || type.Name.EndsWith("Accessor"));
        private bool IsPageType(Type type) => typeof(ISqlPage).IsAssignableFrom(type);
        private bool HasAccessorSuffix(Type type, out string prefix)
        {
            if (type.Name.EndsWith("Accessor") && type.Name.Length > "Accessor".Length)
            {
                prefix = type.Name.Substring(0, type.Name.Length - "Accessor".Length);

                return true;
            }

            prefix = null;

            return false;
        }

        private string SanitizeTypeName(string typeName)
        {
            if (typeName == null)
                return null;

            if (typeName.EndsWith("Async"))
                typeName = typeName.Remove(typeName.Length - "Async".Length);

            if (!typeName.EndsWith("_cssql"))
                typeName += "_cssql";

            return typeName;
        }

        private bool HasPathPrefix(string procName, string prefix, out string suffix)
        {
            suffix = null;

            if (!procName.StartsWith(prefix) || procName.Length <= prefix.Length)
                return false;
            else if (this.IsDirectoryChar(procName[prefix.Length]))
            {
                suffix = procName.Remove(0, prefix.Length + 1);

                return true;
            }

            return false;
        }

        private bool IsRelativePath(string procName, out string path) => (this.HasPathPrefix(procName, ".", out path) || this.HasPathPrefix(procName, "..", out path));
        private bool IsDomainPath(string procName, out string path) => this.HasPathPrefix(procName, "~", out path);
        private bool IsAbsolutePath(string procName, out string path) => this.HasPathPrefix(procName, "", out path);
        private bool IsDirectoryChar(char c) => (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar);
    }
}
