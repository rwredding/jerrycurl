using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Factories
{
    internal delegate TItem AggregateReader<TItem>(IQueryBuffer buffer);
}
