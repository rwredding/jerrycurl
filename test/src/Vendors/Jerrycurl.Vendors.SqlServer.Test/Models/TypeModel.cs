using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Vendors.SqlServer.Test.Models
{
    [Table]
    public class TypeModel
    {
        public bool Bit { get; set; }
        public short SmallInt { get; set; }
        public int Int { get; set; }
        public long BigInt { get; set; }
        public float Real { get; set; }
        public double Float { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime SmallDateTime { get; set; }
        public DateTime DateTime2 { get; set; }
        public TimeSpan Time { get; set; }
        public string NChar { get; set; }
        public string NVarChar { get; set; }
        public string NText { get; set; }
        public string Char { get; set; }
        public string VarChar { get; set; }
        public string Text { get; set; }
        public XDocument Xml { get; set; }
        public byte[] Image { get; set; }
        public byte[] Binary { get; set; }
        public byte[] VarBinary { get; set; }
        public Guid UniqueIdentifier { get; set; }

        public static TypeModel GetSample()
        {
            return new TypeModel()
            {
                BigInt = 8589934592,
                Bit = true,
                Real = 100.0f,
                Int = 100000,
                Float = 100.0d,
                DateTimeOffset = new DateTimeOffset(1819, 5, 4, 3, 2, 1, TimeSpan.FromHours(3)),
                Date = new DateTime(1819, 5, 4),
                DateTime = new DateTime(1819, 5, 4, 3, 2, 1),
                DateTime2 = new DateTime(0019, 5, 4, 3, 2, 1),
                SmallDateTime = new DateTime(1919, 5, 4, 3, 2, 0),
                Time = new TimeSpan(3, 2, 1),
                NChar = "Jerrycurl",
                NVarChar = "Jerrycurl",
                NText = "Jerrycurl",
                Char = "Jerrycurl",
                VarChar = "Jerrycurl",
                Text = "Jerrycurl",
                Xml = XDocument.Parse("<jerry>curl</jerry>"),
                Image = Encoding.ASCII.GetBytes("Jerrycurl"),
                Binary = Encoding.ASCII.GetBytes("Jerrycurl"),
                VarBinary = Encoding.ASCII.GetBytes("Jerrycurl"),
                UniqueIdentifier = new Guid("810f21f7-7e72-48d4-ba67-de5af147c668"),
            };

        }
    }
}
