using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class CascadeSource : IFieldSource
    {
        public CascadeBinding Binding { get; }
        public CommandBuffer Buffer { get; }
        public bool HasChanged => false;

        public object Value
        {
            get => null;
            set { }
        }

        public CascadeSource(CascadeBinding binding, CommandBuffer buffer)
        {
            this.Binding = binding;
            this.Buffer = buffer;
        }

        /*private IFieldSource GetNonCascadingSource()
        {
            IFieldSource source = this.Buffer.GetSources(this.Binding.Source).FirstOrDefault(s => s.HasChanged);
            IFieldSource nextSource = source;

            while (nextSource is CascadeSource cascade)
            {
                nextSource = this.Buffer.GetSource(cascade.Binding.Source);

                if (nextSource == source)
                    throw new InvalidOperationException("Cycle detected.");
            }

            return nextSource;
        }*/
    }
}
