using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Collections;
using Jerrycurl.Data.Commands.Internal.Caching;
using Jerrycurl.Data.Commands.Internal.Compilation;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class FieldPipe
    {
        public IField Target { get; set; }
        public CommandBuffer Buffer { get; }
        public ParameterSource Parameter { get; set; }
        public ColumnSource Column { get; set; }
        public CascadeSource Cascade { get; set; }

        public bool HasChanged => this.GetSources().Any(s => s.HasChanged);

        public FieldPipe(CommandBuffer buffer)
        {
            this.Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void Write(object value)
        {
            if (this.Column != null)
                this.Column.Value = value;
        }

        private IEnumerable<IFieldSource> GetSources() => new IFieldSource[] { this.Column, this.Parameter, this.Cascade }.NotNull();

        public object Read()
        {
            IFieldSource changedSource = this.GetSources().FirstOrDefault(f => f.HasChanged);

            if (changedSource != null)
                return changedSource.Value;

            return DBNull.Value;
        }

        public void Bind()
        {
            if (this.Target != null)
            {
                MetadataIdentity metadata = this.Target.Identity.Metadata;
                ColumnInfo columnInfo = this.Column?.Info;
                BufferConverter converter = CommandCache.GetConverter(metadata, columnInfo);

                this.Target.Bind(converter(this.Read()));
            }
        }
    }
}
