using System;

namespace Jerrycurl.Mvc
{
    public sealed class PageDescriptor
    {
        public Type PageType { get; internal set; }
        public Type DomainType { get; internal set; }
        public Type OriginType { get; internal set; }
        public IProcLocator Locator { get; internal set; }

        internal PageDescriptor()
        {

        }

        public PageDescriptor(Type pageType, Type domainType, Type originType = null, IProcLocator locator = null)
        {
            this.PageType = pageType ?? throw new ArgumentNullException(nameof(pageType));
            this.DomainType = domainType ?? throw new ArgumentNullException(nameof(domainType));
            this.OriginType = originType ?? pageType;
            this.Locator = locator;
        }
    }
}
