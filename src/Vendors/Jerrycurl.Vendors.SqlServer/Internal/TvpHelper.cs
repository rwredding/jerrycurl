using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Microsoft.SqlServer.Server;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Vendors.SqlServer
{
    internal static class TvpHelper
    {
        public static IProjectionMetadata GetPreferredTvpMetadata(IProjection projection)
        {
            if (projection.Metadata.List == null && projection.Metadata.Item == null)
                throw ProjectionException.FromProjection(projection, "No table information found.");

            return projection.Metadata.List ?? projection.Metadata.Item?.List;
        }

        public static IBindingParameterContract GetParameterContract(IBindingMetadata[] bindings)
        {
            ITableMetadata[] columns = bindings.Select(m => m.Identity.GetMetadata<ITableMetadata>()).ToArray();
            ITableMetadata table = columns[0].HasFlag(TableMetadataFlags.Table) ? columns[0] : columns[0].MemberOf;

            string tvpName = string.Join(".", table.TableName);
            string[] columnNames = columns.Select(m => m.ColumnName).ToArray();
            MetadataIdentity[] heading = bindings.Select(m => m.Identity).ToArray();
            BindingParameterConverter[] converters = bindings.Select(m => m?.Parameter?.Convert).ToArray();

            return new BindingParameterContract()
            {
                Write = pi => GetParameterWriterProxy(tvpName, columnNames, heading, converters, pi),
            };
        }

        private static void GetParameterWriterProxy(string tvpName, string[] columnNames, MetadataIdentity[] heading, BindingParameterConverter[] converters, IBindingParameterInfo paramInfo)
        {
            SqlParameter sqlParam = paramInfo.Parameter as SqlParameter ?? throw new InvalidOperationException("Table-valued parameters are only supported on SqlParameter instances.");

            Relation relation = new Relation(paramInfo.Field, heading);

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
