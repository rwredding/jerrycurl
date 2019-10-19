using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public sealed class DomainDescriptor
    {
        public Type DomainType { get; internal set; }
        public Type OriginType { get; internal set; }

        internal DomainDescriptor()
        {

        }

        public DomainDescriptor(Type domainType, Type originType = null)
        {
            this.DomainType = domainType ?? throw new ArgumentNullException(nameof(domainType));
            this.OriginType = originType ?? domainType;
        }
    }
}
