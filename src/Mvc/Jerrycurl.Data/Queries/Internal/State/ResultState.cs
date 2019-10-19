using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.State
{
    internal class ResultState<TItem>
    {
        public AggregateState Aggregate { get; set; }
        public Action<ExpandingArray, ExpandingArray, ExpandingArray<bool>> Initializer { get; set; }
        public Action<IDataReader, ExpandingArray, ExpandingArray, ExpandingArray<bool>> List { get; set; }
        public Action<IDataReader, ExpandingArray, ExpandingArray> ListItem { get; set; }
        public Func<IDataReader, TItem> Item { get; set; }
    }
}
