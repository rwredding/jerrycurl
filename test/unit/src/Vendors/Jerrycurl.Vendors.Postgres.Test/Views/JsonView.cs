using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Vendors.Postgres.Test.Models;

namespace Jerrycurl.Vendors.Postgres.Test.Views
{
    [Table]
    public class JsonView
    {
        public JsonModel Json { get; set; }
        public JsonModel JsonB { get; set; }
    }
}
