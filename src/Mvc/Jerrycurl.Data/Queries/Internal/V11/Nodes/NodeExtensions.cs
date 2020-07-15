using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    internal static class NodeExtensions
    {
        public static bool IsContainer(this Node node) => node.Flags.HasFlag(NodeFlags.Container);
        public static bool IsValue(this Node node) => node.Flags.HasFlag(NodeFlags.Value);
    }
}
