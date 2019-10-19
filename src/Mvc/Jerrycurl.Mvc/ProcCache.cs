using System;
using System.Collections.Concurrent;

namespace Jerrycurl.Mvc
{
    internal static class ProcCache
    {
        public static ConcurrentDictionary<ProcCacheKey, ProcFactory> Procs { get; } = new ConcurrentDictionary<ProcCacheKey, ProcFactory>();
        public static ConcurrentDictionary<Type, PageFactory> Pages { get; } = new ConcurrentDictionary<Type, PageFactory>();
        public static ConcurrentDictionary<Type, Lazy<IDomainOptions>> Domains { get; } = new ConcurrentDictionary<Type, Lazy<IDomainOptions>>();

        public static ConcurrentDictionary<PageLocatorKey, PageDescriptor> PageDescriptors { get; } = new ConcurrentDictionary<PageLocatorKey, PageDescriptor>();
        public static ConcurrentDictionary<Type, DomainDescriptor> DomainDescriptors { get; } = new ConcurrentDictionary<Type, DomainDescriptor>();
    }
}
