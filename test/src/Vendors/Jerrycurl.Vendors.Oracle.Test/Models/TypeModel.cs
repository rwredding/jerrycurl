using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Vendors.Oracle.Test.Models
{
    [Table]
    public class TypeModel
    {
        public decimal Number { get; set; }
        public DateTime Date { get; set; }
        public string Char { get; set; }
        public string VarChar { get; set; }
        public string VarChar2 { get; set; }
        public string Clob { get; set; }
        public string NClob { get; set; }
        public string Long { get; set; }
        public string NChar { get; set; }
        public string NVarChar2 { get; set; }
        public byte[] Blob { get; set; }
        public byte[] Raw { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime TimeStampLz { get; set; }
        public DateTimeOffset TimeStampTz { get; set; }
        public TimeSpan IntervalDS { get; set; }

        public static TypeModel GetSample()
        {
            return new TypeModel()
            {
                Date = new DateTime(0019, 5, 4, 3, 2, 1),
                Char = "Jerrycurl",
                Clob = "Jerrycurl",
                Long = "Jerrycurl",
                NChar = "Jerrycurl",
                NClob = "Jerrycurl",
                NVarChar2 = "Jerrycurl",
                VarChar = "Jerrycurl",
                VarChar2 = "Jerrycurl",
                Blob = Encoding.ASCII.GetBytes("Jerrycurl"),
                Raw = Encoding.ASCII.GetBytes("Jerrycurl"),
                Number = 100.03153433m,
                TimeStampTz = new DateTimeOffset(1919, 5, 4, 3, 2, 1, TimeSpan.FromHours(3)),
                TimeStamp = new DateTime(0019, 5, 4, 3, 2, 1),
                TimeStampLz = new DateTime(0019, 5, 4, 3, 2, 1),
                IntervalDS = new TimeSpan(2, 3, 4, 11, 333),
                //IntervalYM = 254,
            };

        }
    }
}
