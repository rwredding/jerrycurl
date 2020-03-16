using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Vendors.Postgres.Test.Models
{
    [Table]
    public class TestModel
    {
        public JsonModel Json { get; set; }
        public JsonModel JsonB { get; set; }
    }
}
