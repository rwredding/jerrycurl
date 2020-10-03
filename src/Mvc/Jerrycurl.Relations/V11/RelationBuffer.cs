
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.V11.Internal.Compilation;
using Jerrycurl.Relations.V11.Internal.Enumerators;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.V11
{
    internal class RelationBuffer
    {
        public BufferWriter Writer { get; set; }
        public IField2 Model { get; set; }
        public IField2 Source { get; set; }
        public IRelationQueue[] Queues { get; set; }
        public IField2[] Fields { get; set; }
    }
}
