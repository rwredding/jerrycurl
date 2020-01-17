using System.Collections.Generic;

namespace Jerrycurl.Relations.Internal
{
    internal class ProductEnumerator<T> : IndexedEnumerator<T>
    {
        private readonly List<T> cachedList = new List<T>();
        private bool isCached = false;

        public ProductEnumerator(IEnumerable<T> enumerable)
            : base(enumerable)
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
