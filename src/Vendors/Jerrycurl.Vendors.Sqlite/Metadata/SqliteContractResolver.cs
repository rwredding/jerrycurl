using System;
using System.Data;
using System.Reflection;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Vendors.Sqlite.Metadata
{
    public class SqliteContractResolver : IBindingContractResolver
    {
        public int Priority => 1000;
        public IBindingCompositionContract GetCompositionContract(IBindingMetadata metadata) => null;
        public IBindingParameterContract GetParameterContract(IBindingMetadata metadata) => null;
        public IBindingHelperContract GetHelperContract(IBindingMetadata metadata) => null;
        public IBindingValueContract GetValueContract(IBindingMetadata metadata)
        {
            return new BindingValueContract()
            {
                Read = this.GetRecordReaderMethod,
                Convert = metadata.Value.Convert,
            };
        }

        private MethodInfo GetRecordMethod(string methodName) => typeof(IDataRecord).GetMethod(methodName, new[] { typeof(int) });

        private MethodInfo GetRecordReaderMethod(IBindingColumnInfo columnInfo)
        {
            switch (columnInfo.Column.TypeName?.ToLower())
            {
                case "integer" when columnInfo.Column.Type == typeof(long):
                    return this.GetRecordMethod(nameof(IDataReader.GetInt64));
                case "int":
                case "integer":
                case "mediumint":
                    return this.GetRecordMethod(nameof(IDataReader.GetInt32));
                case "tinyint":
                    return this.GetRecordMethod(nameof(IDataReader.GetByte));
                case "smallint":
                    return this.GetRecordMethod(nameof(IDataReader.GetInt16));
                case "unsigned big int":
                case "bigint":
                    return this.GetRecordMethod(nameof(IDataReader.GetInt64));
                case "nvarchar":
                case "varchar":
                case "varying character":
                case "nchar":
                case "native character":
                case "text":
                case "clob":
                    return this.GetRecordMethod(nameof(IDataReader.GetString));
                case "float":
                    return this.GetRecordMethod(nameof(IDataReader.GetFloat));
                case "decimal":
                case "double precision":
                    return this.GetRecordMethod(nameof(IDataReader.GetDouble));
                case "datetime":
                case "date":
                    return this.GetRecordMethod(nameof(IDataReader.GetDateTime));
                case "boolean":
                    return this.GetRecordMethod(nameof(IDataReader.GetBoolean));
                case "blob":
                    return this.GetBlobReaderMethod(columnInfo.Metadata);
            }

            return null;
        }

        private MethodInfo GetBlobReaderMethod(IBindingMetadata metadata)
        {
            Type dataType = Nullable.GetUnderlyingType(metadata.Type) ?? metadata.Type;

            if (dataType == typeof(long))
                return this.GetRecordMethod(nameof(IDataReader.GetInt64));
            else if (dataType == typeof(int))
                return this.GetRecordMethod(nameof(IDataReader.GetInt32));
            else if (dataType == typeof(short))
                return this.GetRecordMethod(nameof(IDataReader.GetInt16));
            else if (dataType == typeof(DateTime))
                return this.GetRecordMethod(nameof(IDataReader.GetDateTime));
            else if (dataType == typeof(byte))
                return this.GetRecordMethod(nameof(IDataReader.GetByte));
            else if (dataType == typeof(float))
                return this.GetRecordMethod(nameof(IDataReader.GetFloat));
            else if (dataType == typeof(double))
                return this.GetRecordMethod(nameof(IDataReader.GetDouble));
            else if (dataType == typeof(string))
                return this.GetRecordMethod(nameof(IDataReader.GetString));
            else if (dataType == typeof(bool))
                return this.GetRecordMethod(nameof(IDataReader.GetBoolean));

            return null;
        }
    }
}
