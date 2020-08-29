using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class SetReader : ValueReader
    {
        public int? EnumeratorIndex { get; set; }

        public SetReader(Node node)
            : base(node)
        {
            
        }

        public override string ToString() => this.Metadata.Identity.Name;
    }
}
