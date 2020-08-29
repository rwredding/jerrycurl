using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Caching;
using Jerrycurl.Relations.Internal.V11.Compilation;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11
{
    internal class RelationReader : IRelationReader
    {
        public IField2 Source { get; }
        public int Degree { get; }
        public IField2[] Buffer { get; }
        public Relation2 Relation { get; }

        IRelation2 IRelationReader.Relation => this.Relation;

        private int currentIndex;
        private BufferWriter[] writers;
        private IEnumerator[] enumerators;

        public RelationReader(Relation2 relation)
        {
            this.Degree = relation.Identity.Heading.Count;
            this.Relation = relation;
            this.Source = relation.Source;
        }

        public IField2 this[int index]
        {
            get
            {
                if (index < 0 || index >= this.Degree)
                    throw new IndexOutOfRangeException();

                return this.Buffer[index];
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < this.enumerators.Length; i++)
            {
                try
                {
                    if (this.enumerators[i] is IDisposable disposable)
                        disposable.Dispose();

                    this.enumerators[i] = null;
                }
                catch { }
            }
        }

        public bool Read()
        {
            BufferWriter[] writers = this.writers ??= RelationCache.GetWriters(this.Source.Identity.Metadata, this.Relation.Identity);
            IEnumerator[] enumerators = this.enumerators ??= new IEnumerator[this.writers.Length - 1];

            writers[0](this.Buffer, this.enumerators);

            while (this.currentIndex >= 0)
            {
                if (this.currentIndex == enumerators.Length)
                {
                    this.currentIndex--;

                    return true;
                }
                else if (this.MoveNextOrThrow(enumerators[this.currentIndex], null))
                {
                    writers[this.currentIndex + 1](this.Buffer, this.enumerators);

                    this.currentIndex++;
                }
                else
                {
                    if (enumerators[this.currentIndex] is IDisposable disposable)
                        disposable.Dispose();

                    this.currentIndex--;
                }
            }

            return false;
        }


        private bool MoveNextOrThrow(IEnumerator enumerator, MetadataIdentity identity)
        {
            if (enumerator == null)
                return false;

            try
            {
                return enumerator.MoveNext();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException();
                //throw RelationException.FromRelation(this.Relation, $"Cannot move enumerator for '{identity}': {ex.Message}", ex);
            }
        }
    }
}
