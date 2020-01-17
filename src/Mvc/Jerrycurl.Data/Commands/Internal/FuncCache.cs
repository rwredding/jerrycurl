using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class FuncCache
    {
        private static readonly ConcurrentDictionary<FuncTableKey, Action<IDataReader, FieldData[]>> tableMap = new ConcurrentDictionary<FuncTableKey, Action<IDataReader, FieldData[]>>();
        private static readonly ConcurrentDictionary<FuncColumnKey, Action<IField, object>> columnMap = new ConcurrentDictionary<FuncColumnKey, Action<IField, object>>();

        public static Action<IDataReader, FieldData[]> GetFieldDataBinder(MetadataIdentity[] metadata, TableIdentity tableInfo)
        {
            FuncTableKey key = new FuncTableKey(metadata, tableInfo);

            return tableMap.GetOrAdd(key, _ =>
            {
                FuncCompiler compiler = new FuncCompiler(tableInfo, metadata);

                return compiler.Compile();
            });
        }

        public static Action<IField, object> GetFieldBinder(MetadataIdentity metadata, ColumnIdentity column)
        {
            FuncColumnKey key = new FuncColumnKey(metadata, column);

            return columnMap.GetOrAdd(key, _ => BuildFieldBinder(metadata, column));
        }

        private static Action<IField, object> BuildFieldBinder(MetadataIdentity metadata, ColumnIdentity column)
        {
            ParameterExpression fieldParam = Expression.Parameter(typeof(IField));
            ParameterExpression valueParam = Expression.Parameter(typeof(object));

            IBindingMetadata binding = metadata.GetMetadata<IBindingMetadata>();

            Expression bindValue = valueParam;

            if (metadata != null)
            {
                Type sourceType = null;

                if (column != null)
                {
                    BindingColumnInfo columnInfo = new BindingColumnInfo()
                    {
                        Column = column,
                        CanBeNull = true,
                        Metadata = binding,
                    };

                    sourceType = binding.Value?.Read(columnInfo)?.ReturnType;
                }

                BindingValueInfo valueInfo = new BindingValueInfo()
                {
                    CanBeNull = true,
                    CanBeDbNull = true,
                    Metadata = binding,
                    Value = bindValue,
                    SourceType = sourceType,
                    TargetType = binding.Type,
                };

                bindValue = binding.Value?.Convert?.Invoke(valueInfo) ?? valueParam;
            }

            MethodInfo bindMethod = typeof(IField).GetMethod(nameof(IField.Bind), new[] { typeof(object) });

            Expression bindCall = Expression.Call(fieldParam, bindMethod, Expression.Convert(bindValue, typeof(object)));

            return Expression.Lambda<Action<IField, object>>(bindCall, fieldParam, valueParam).Compile();
        }
    }
}
