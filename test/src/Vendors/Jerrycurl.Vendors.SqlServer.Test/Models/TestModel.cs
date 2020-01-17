using System.Collections.Generic;

namespace Jerrycurl.Vendors.SqlServer.Test.Models
{
    public class TestModel
    {
        public JsonModel Json { get; set; }
        public IList<TvpModel> Tvp { get; set; }
        public string JsonString { get; set; }
    }
}
