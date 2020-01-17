using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class FuncCompiler
    {
        private readonly MetadataIdentity[] attributes;
        private readonly ColumnIdentity[] columns;
        private readonly FuncArguments args = new FuncArguments();

        public FuncCompiler(TableIdentity heading, MetadataIdentity[] attributes)
        {
            this.columns = heading.Columns.ToArray();
            this.attributes = attributes;
        }

        public Action<IDataReader, FieldData[]> Compile()
        {
            if (this.columns.Length == 0 || this.attributes.All(m => m == null))
                return (dr, fd) => { };

            List<Expression> body = new List<Expression>();

            for (int i = 0; i < this.columns.Length; i++)
            {
                if (this.attributes[i] != null)
                {
                    Expression value = this.GetRawValueExpression(i);
                    Expression binder = this.GetBinder(i, value);

                    body.Add(binder);
                }

            }

            Expression block = Expression.Block(body);

            ParameterExpression[] parameters = new ParameterExpression[] { this.args.DataReader, this.args.Fields, this.args.Columns };

            Action<IDataReader, FieldData[], ColumnIdentity[]> fullFunc = Expression.Lambda<Action<IDataReader, FieldData[], ColumnIdentity[]>>(block, this.args.GetParameters()).Compile();

            ColumnIdentity[] cells = this.columns;

            return (dr, fd) => fullFunc(dr, fd, cells);
        }

        private Expression GetRawValueExpression(int index)
        {
            MethodInfo readMethod = this.GetValueReadMethod(index);
            MethodInfo nullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new[] { typeof(int) });

            Expression readIndex = Expression.Constant(index);
            Expression dataReader = this.args.DataReader;

            if (readMethod.DeclaringType != typeof(IDataReader) && readMethod.DeclaringType != typeof(IDataRecord))
                dataReader = Expression.Convert(dataReader, readMethod.DeclaringType);

            Expression readValue = Expression.Call(dataReader, readMethod, readIndex);
            Expression isDbNull = Expression.Call(dataReader, nullMethod, readIndex);
            Expression dbNull = Expression.Field(null, typeof(DBNull).GetField(nameof(DBNull.Value)));

            Expression nullObject = Expression.Convert(dbNull, typeof(object));
            Expression readObject = Expression.Convert(readValue, typeof(object));

            return Expression.Condition(isDbNull, nullObject, readObject);
        }

        private MethodInfo GetValueReadMethod(int index)
        {
            BindingColumnInfo bindingInfo = new BindingColumnInfo()
            {
                Metadata = this.attributes[index].GetMetadata<IBindingMetadata>(),
                Column = this.columns[index],
            };

            MethodInfo readMethod = bindingInfo.Metadata?.Value?.Read?.Invoke(bindingInfo);

            if (readMethod == null)
                readMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), new Type[] { typeof(int) });

            return readMethod;
        }

        private Expression GetBinder(int index, Expression value)
        {
            Type fieldType = typeof(FieldData);

            MethodInfo setValue = fieldType.GetMethod(nameof(FieldData.SetValue), new[] { typeof(object) });
            MethodInfo setCell = fieldType.GetMethod(nameof(FieldData.SetCellInfo), new[] { typeof(ColumnIdentity) });

            Expression fieldData = Expression.ArrayAccess(this.args.Fields, Expression.Constant(index));
            Expression cellInfo = Expression.ArrayAccess(this.args.Columns, Expression.Constant(index));

            Expression objectValue = this.GetObjectExpression(value);

            Expression set1 = Expression.Call(fieldData, setValue, objectValue);
            Expression set2 = Expression.Call(fieldData, setCell, cellInfo);

            return Expression.Block(set1, set2);
        }

        private Expression GetObjectExpression(Expression expression)
        {
            if (expression.Type.IsValueType)
                return Expression.Convert(expression, typeof(object));

            return expression;
        }
    }
}
