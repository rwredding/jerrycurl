using System.Collections;
using System.Collections.Generic;

namespace Jerrycurl.Relations
{
    public class Many<T> : IEnumerable<T>
    {
        private T value = default;

        public bool HasValue { get; private set; }
        public T Value
        {
            get => this.value;
            set
            {
                this.value = value;
                this.HasValue = true;
            }
        }

        public Many() { }
        public Many(T value)
        {
            this.value = value;
            this.HasValue = true;
        }

        public void Clear()
        {
            this.value = default;
            this.HasValue = false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this.HasValue)
                yield return this.value;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
