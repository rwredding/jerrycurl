using System.Text;
using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Vendors.Oracle.Test.Models
{
    [Table]
    public class TypeModel2
    {
        public byte[] LongRaw { get; set; }

        public static TypeModel2 GetSample()
        {
            return new TypeModel2()
            {
                LongRaw = Encoding.ASCII.GetBytes("Jerrycurl"),
            };

        }
    }
}
