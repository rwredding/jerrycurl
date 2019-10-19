using System;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Mvc
{
    internal class PageLocatorKey : IEquatable<PageLocatorKey>
    {
        public string ProcName { get; }
        public Type OriginType { get; }

        public PageLocatorKey(string procName, Type originType)
        {
            this.ProcName = procName;
            this.OriginType = originType;
        }

        public bool Equals(PageLocatorKey other) => Equality.Combine(this, other, m => m.ProcName, m => m.OriginType);
        public override int GetHashCode() => HashCode.Combine(this.ProcName, this.OriginType);
        public override bool Equals(object obj) => (obj is PageLocatorKey other && this.Equals(other));
    }
}
