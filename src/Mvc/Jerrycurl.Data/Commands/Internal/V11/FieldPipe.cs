using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands.Internal.V11
{
    internal class FieldPipe
    {
        public IFieldSource Source { get; set; }
        public HashSet<IField> Targets { get; } = new HashSet<IField>();
        public CommandBuffer Buffer { get; }

        public FieldPipe(CommandBuffer buffer)
        {
            this.Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void Write(object value)
        {
            if (this.Source != null)
                this.Source.Value = value;
        }

        public void Bind()
        {

        }
    }
}
