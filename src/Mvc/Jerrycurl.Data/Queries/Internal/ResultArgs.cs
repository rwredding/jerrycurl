using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Collections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class ResultArgs
    {
        public static ParameterExpression DataReader { get; } = Expression.Parameter(typeof(IDataReader), "dataReader");
        public static ParameterExpression Lists { get; } = Expression.Parameter(typeof(ExpandingArray), "lists");
        public static ParameterExpression Dicts { get; } = Expression.Parameter(typeof(ExpandingArray), "dicts");
        public static ParameterExpression Bits { get; } = Expression.Parameter(typeof(ExpandingArray<bool>), "bits");
        public static ParameterExpression SchemaType { get; } = Expression.Parameter(typeof(Type), "schemaType");
        public static ParameterExpression Helpers { get; } = Expression.Parameter(typeof(ExpandingArray), "helpers");
    }
}
