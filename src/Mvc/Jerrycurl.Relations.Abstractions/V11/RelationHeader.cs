using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.V11
{
    public class RelationHeader
    {
        public ISchema Schema { get; }
        public IReadOnlyList<RelationAttribute> Attributes { get; }

        public RelationHeader(ISchema schema, IReadOnlyList<RelationAttribute> attributes)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public override string ToString() => $"{this.Schema}({string.Join(", ", this.Attributes.Select(a => $"\"{a.Identity.Name}\""))})";
    }
}
