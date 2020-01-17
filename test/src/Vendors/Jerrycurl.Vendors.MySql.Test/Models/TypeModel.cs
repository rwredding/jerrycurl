using System;
using System.Text;
using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Vendors.MySql.Test.Models
{
    [Table]
    public class TypeModel
    {
        public sbyte TinyInt { get; set; }
        public short SmallInt { get; set; }
        public int MediumInt { get; set; }
        public int Int { get; set; }
        public long BigInt { get; set; }

        public byte UTinyInt { get; set; }
        public ushort USmallInt { get; set; }
        public uint UMediumInt { get; set; }
        public uint UInt { get; set; }
        public ulong UBigInt { get; set; }

        public float Float { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime TimeStamp { get; set; }
        public TimeSpan Time { get; set; }
        public short Year { get; set; }
        public string Enum { get; set; }
        public string Char { get; set; }
        public string VarChar { get; set; }
        public string TinyText { get; set; }
        public string Text { get; set; }
        public string MediumText { get; set; }
        public string LongText { get; set; }
        public byte[] Binary { get; set; }
        public byte[] VarBinary { get; set; }
        public byte[] TinyBlob { get; set; }
        public byte[] Blob { get; set; }
        public byte[] MediumBlob { get; set; }
        public byte[] LongBlob { get; set; }
        public string Set { get; set; }

        public static TypeModel GetSample()
        {
            return new TypeModel()
            {
                BigInt = 8589934592,
                TinyInt = 66,
                Double = 100.0d,
                Int = 100000,
                Float = 100.0f,
                Decimal = 100.03153433m,
                Date = new DateTime(1819, 5, 4),
                DateTime = new DateTime(1019, 5, 4, 3, 2, 1),
                TimeStamp = new DateTime(1979, 5, 4, 3, 2, 1),
                Time = new TimeSpan(3, 2, 1),
                Char = "Jerrycurl",
                VarChar = "Jerrycurl",
                Text = "Jerrycurl",
                Blob = Encoding.ASCII.GetBytes("Jerrycurl"),
                LongBlob = Encoding.ASCII.GetBytes("Jerrycurl"),
                MediumBlob = Encoding.ASCII.GetBytes("Jerrycurl"),
                TinyBlob = Encoding.ASCII.GetBytes("Jerrycurl"),
                MediumInt = 60000,
                LongText = "Jerrycurl",
                MediumText = "Jerrycurl",
                TinyText = "Jerrycurl",
                SmallInt = 32000,
                UBigInt = 561616311896,
                UInt = 18668423,
                UMediumInt = 15616,
                USmallInt = 8161,
                UTinyInt = 200,
                Year = 2019,
                Binary = Encoding.ASCII.GetBytes("Jerrycurl"),
                VarBinary = Encoding.ASCII.GetBytes("Jerrycurl"),
                Enum = "Jerrycurl",
                Set = "Jerrycurl",
            };
        }
    }
}
