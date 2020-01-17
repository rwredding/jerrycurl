using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries
{
    public class QueryOptions : SessionOptions
    {
        public ISchemaStore Schemas { get; set; }
    }
}
