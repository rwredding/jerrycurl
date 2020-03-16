using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Vendors.Oracle.Test.Models;

namespace Jerrycurl.Vendors.Oracle.Test.Views
{
    [Table]
    public class JsonView
    {
        public JsonModel Json { get; set; }
        public string JsonString { get; set; }
    }
}
