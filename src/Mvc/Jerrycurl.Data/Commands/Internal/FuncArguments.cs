using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class FuncArguments
    {
        public ParameterExpression DataReader { get; } = Expression.Parameter(typeof(IDataReader), "dataReader");
        public ParameterExpression Fields { get; } = Expression.Parameter(typeof(FieldData[]), "fields");
        public ParameterExpression Columns { get; } = Expression.Parameter(typeof(ColumnIdentity[]), "columns");

        public IEnumerable<ParameterExpression> GetParameters()
        {
            yield return this.DataReader;
            yield return this.Fields;
            yield return this.Columns;
        }
    }
}
