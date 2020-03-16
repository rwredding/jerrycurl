using System.Collections.Generic;

namespace Jerrycurl.Vendors.SqlServer.Test.Models
{
    public class TvpResult
    {
        public IList<int> Products { get; set; }
        public IList<int?> Ints { get; set; }
        public IList<int> Keys { get; set; }
    }
}
