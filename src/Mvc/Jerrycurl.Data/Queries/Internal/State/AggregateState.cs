using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.State
{
    internal class AggregateState
    {
        private readonly HashSet<MetadataIdentity> targets = new HashSet<MetadataIdentity>();
        private readonly object lockState = new object();

        public IReadOnlyList<MetadataIdentity> Targets => this.targets.ToList();
        public Action<ExpandingArray, ExpandingArray<bool>> Factory { get; private set; }
        public TypeState Type { get; }

        public AggregateState(TypeState type)
        {
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public void Add(MetadataIdentity identity)
        {
            if (!this.targets.Contains(identity))
            {
                lock (this.lockState)
                {
                    this.targets.Add(identity);

                    this.UpdateFactory();
                }
            }
        }

        public void UpdateFactory()
        {
            this.Factory = new ResultCompiler(this.Type).CompileAggregate();
        }
    }
}
