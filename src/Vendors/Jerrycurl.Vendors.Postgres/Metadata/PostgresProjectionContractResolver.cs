using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Metadata.Annotations;
using Npgsql;
using NpgsqlTypes;

namespace Jerrycurl.Vendors.Postgres.Metadata
{
    public class PostgresProjectionContractResolver : IProjectionContractResolver
    {
        public ProjectionMetadataFlags GetFlags(IProjectionMetadata metadata) => (metadata.Flags & ~ProjectionMetadataFlags.Output);
    }
}
