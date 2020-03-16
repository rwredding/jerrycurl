using Jerrycurl.Mvc.Metadata.Annotations;
using Newtonsoft.Json;

namespace Jerrycurl.Vendors.Oracle.Test.Models
{
    [Json]
    public class JsonModel
    {
        public int Value1 { get; set; }
        [JsonProperty(PropertyName = "Value2")]
        public int Value3 { get; set; }
    }
}
