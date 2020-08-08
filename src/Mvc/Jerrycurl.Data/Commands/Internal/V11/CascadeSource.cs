using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands.Internal.V11
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

        //private IFieldSource GetNonCascadingSource()
        //{
        //    IFieldSource source = this.Buffer.GetSource(this.Binding.Source);
        //    IFieldSource nextSource = source;

        //    while (nextSource is CascadeSource cascade)
        //    {
        //        nextSource = this.Buffer.GetSource(cascade.Binding.Source);

        //        if (nextSource == source)
        //            throw new InvalidOperationException("Cycle detected.");
        //    }

        //    return nextSource;
        //}
    }
}
