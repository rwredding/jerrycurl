using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Vendors.SqlServer.Internal;
#if SQLSERVER_LEGACY
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
#else
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
#endif

namespace Jerrycurl.Vendors.SqlServer
{
    public class TableValuedParameter : IParameter
    {
        public string Name { get; }
        public IRelation Relation { get; }

        IField IParameter.Field => null;

        public TableValuedParameter(string name, IRelation relation)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        }

        public void Build(IDbDataParameter adoParameter)
        {
            SqlParameter sqlParam = adoParameter as SqlParameter ?? throw new InvalidOperationException("Table-valued parameters are only supported on SqlParameter instances.");

            sqlParam.ParameterName = this.Name;

            Action<SqlParameter, IRelation> binder = TvpCache.Binders.GetOrAdd(this.Relation.Identity, key =>
            {
                ITableMetadata[] columnMetadata = key.Heading.Select(m => m.GetMetadata<ITableMetadata>()).Where(m => m.HasFlag(TableMetadataFlags.Column)).ToArray();
                IBindingMetadata[] bindingMetadata = columnMetadata.Select(m => m.Identity.GetMetadata<IBindingMetadata>()).ToArray();

                if (bindingMetadata.Length == 0)
                    throw new InvalidOperationException("No columns found.");

                ITableMetadata tableMetadata = columnMetadata[0].HasFlag(TableMetadataFlags.Table) ? columnMetadata[0] : columnMetadata[0].MemberOf;

                string tvpName = string.Join(".", tableMetadata.TableName);
                string[] columnNames = columnMetadata.Select(m => m.ColumnName).ToArray();
                BindingParameterConverter[] converters = bindingMetadata.Select(m => m?.Parameter?.Convert).ToArray();

                return (sp, r) => BindParameter(sp, tvpName, columnNames,  converters, r);
            });

            binder(sqlParam, this.Relation);
        }


        private static void BindParameter(SqlParameter sqlParam, string tvpName, string[] columnNames, BindingParameterConverter[] converters, IRelation relation)
        {
            ITuple refTuple = relation.Row();

            IEnumerable<SqlDataRecord> iterator()
            {
                SqlDataRecord record = GetDataRecord(columnNames, refTuple);

                foreach (ITuple tuple in relation)
                {
                    SetRecordValues(record, tuple, converters);

                    yield return record;
                }
            }

            if (refTuple == null)
                sqlParam.Value = Array.Empty<SqlDataRecord>();
            else
                sqlParam.Value = iterator();

            sqlParam.SqlDbType = SqlDbType.Structured;
            sqlParam.TypeName = tvpName;
        }

        private static SqlDataRecord GetDataRecord(string[] columnNames, ITuple tuple)
        {
            SqlMetaData[] columns = columnNames.Zip(tuple, (n, f) => SqlMetaData.InferFromValue(f.Value, n)).ToArray();

            return new SqlDataRecord(columns);
        }

        private static void SetRecordValues(SqlDataRecord record, ITuple tuple, BindingParameterConverter[] converters)
        {
            for (int i = 0; i < record.FieldCount; i++)
            {
                object value = converters[i]?.Invoke(tuple[i].Value) ?? tuple[i].Value;

                record.SetValue(i, value);
            }
        }
    }
}
