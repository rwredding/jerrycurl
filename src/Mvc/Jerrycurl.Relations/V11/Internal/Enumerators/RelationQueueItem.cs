using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11.Internal.Enumerators
{
    internal class RelationQueueItem<TList> : NameBuffer
    {
        public TList List { get; }

        public RelationQueueItem(TList list, string namePart, DotNotation2 notation)
            : base(namePart, notation)
        {
            this.List = list;
        }
    }
}
