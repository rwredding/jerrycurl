using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata.Annotations;
using Npgsql;
using NpgsqlTypes;

namespace Jerrycurl.Vendors.Postgres.Metadata
{
    public class PostgresContractResolver : IBindingContractResolver
    {
        public int Priority => 1000;

        private MethodInfo GetNpgsqlReaderMethod(string methodName)
        {
            Type reader = typeof(NpgsqlDataReader);

            return reader.GetMethod(methodName, new[] { typeof(int) }) ?? throw new InvalidOperationException("No such method.");
        }

        private MethodInfo GetValueReaderProxy(IBindingColumnInfo columnInfo, IBindingValueContract fallback)
        {
            if (columnInfo.Column.Type == typeof(TimeSpan))
                return this.GetNpgsqlReaderMethod(nameof(NpgsqlDataReader.GetTimeSpan));

            return fallback?.Read(columnInfo);
        }

        public IBindingParameterContract GetParameterContract(IBindingMetadata metadata)
        {
            IBindingParameterContract fallback = metadata.Parameter;

            if (metadata.Annotations.OfType<JsonAttribute>().Any())
            {
                return new BindingParameterContract()
                {
                    Convert = fallback.Convert,
                    Write = pi =>
                    {
                        fallback?.Write?.Invoke(pi);

                        if (pi.Parameter is NpgsqlParameter npgParam)
                            npgParam.NpgsqlDbType = NpgsqlDbType.Json;
                    }
                };
            }
            else if (metadata.Type == typeof(XDocument))
            {
                return new BindingParameterContract()
                {
                    Convert = fallback.Convert,
                    Write = pi =>
                    {
                        fallback.Write(pi);

                        if (pi.Parameter is NpgsqlParameter npgParam)
                            npgParam.NpgsqlDbType = NpgsqlDbType.Xml;
                    }
                };
            }

            return null;
        }

        public IBindingValueContract GetValueContract(IBindingMetadata metadata)
        {
            IBindingValueContract fallback = metadata.Value;

            return new BindingValueContract()
            {
                Convert = fallback.Convert,
                Read = ci => this.GetValueReaderProxy(ci, fallback),
            };
        }

        public IBindingCompositionContract GetCompositionContract(IBindingMetadata metadata) => null;
        public IBindingHelperContract GetHelperContract(IBindingMetadata metadata) => null;
    }
}
