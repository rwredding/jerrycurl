using System;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Mvc
{
    internal class ProcCacheKey : IEquatable<ProcCacheKey>
    {
        public Type PageType { get; }
        public Type ModelType { get; }
        public Type ResultType { get; }

        public ProcCacheKey(PageDescriptor descriptor, ProcArgs args)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            this.PageType = descriptor.PageType;
            this.ModelType = args.ModelType;
            this.ResultType = args.ResultType;
        }

        public bool Equals(ProcCacheKey other) => Equality.Combine(this, other, m => m.PageType, m => m.ModelType, m => m.ResultType);
        public override int GetHashCode() => HashCode.Combine(this.PageType, this.ModelType, this.ResultType);
        public override bool Equals(object obj) => (obj is ProcCacheKey other && this.Equals(other));
    }
}
