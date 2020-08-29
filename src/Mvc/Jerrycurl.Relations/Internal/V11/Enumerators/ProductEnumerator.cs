using System.Collections.Generic;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.Enumerators
{
    internal class CachedEnumerator<T> : IndexedEnumerator<T>
    {
        private readonly List<T> cachedList = new List<T>();
        private bool isCached = false;

        public CachedEnumerator(IEnumerable<T> enumerable, string itemName, IMetadataNotation notation)
            : base(enumerable, itemName, notation)
        {

        }

        public override bool MoveNext()
        {
            if (!this.isCached)
            {
                bool result;

                if (result = base.MoveNext())
                    this.cachedList.Add(this.Current);

                return result;
            }

            return base.MoveNext();
        }

        public override void Dispose()
        {
            base.Dispose();

            this.innerEnumerator = this.cachedList.GetEnumerator();
            this.isCached = true;
        }
    }
}
