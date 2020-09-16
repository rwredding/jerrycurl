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
    internal class FieldBuffer
    {
        public IField Target { get; set; }
        public ParameterSource Parameter { get; set; }
        public ColumnSource Column { get; set; }
        public CascadeSource Cascade { get; set; }

        public bool HasChanges() => this.GetChanges().Any();
        public IEnumerable<IFieldSource> GetSources() => new IFieldSource[] { this.Column, this.Parameter, this.Cascade }.NotNull();
        public IEnumerable<IFieldSource> GetChanges() => this.GetSources().Where(s => s.HasChanged);

        public bool Read(out object value)
        {
            IFieldSource changedSource = this.GetSources().FirstOrDefault(f => f.HasChanged);

            if (changedSource != null)
            {
                value = changedSource.Value;

                return true;
            }

            value = null;

            return false;
        }

        public void Write(object value)
        {
            if (this.Column != null)
                this.Column.Value = value;
        }

        public void Bind()
        {
            if (this.Target != null && this.Read(out object value))
            {
                MetadataIdentity metadata = this.Target.Identity.Metadata;
                ColumnMetadata columnInfo = this.Column?.Info;
                BufferConverter converter = CommandCache.GetConverter(metadata, columnInfo);

                this.Target.Bind(converter(value));
            }
        }
    }
}
