using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.V11.Internal.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.V11.Internal.IO
{
    internal class PropertyReader : NodeReader
    {
        public PropertyReader(Node node)
            : base(node)
        {
            
        }

        public override string ToString() => this.Metadata.Identity.Name;
    }
}
