using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml.Linq;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Mvc.Metadata.Annotations;

namespace Jerrycurl.Vendors.Postgres.Test.Models
{
    [Table]
    public class TypeModel
    {
        public short SmallInt { get; set; }
        public int Integer { get; set; }
        public long BigInt { get; set; }
        public double Double { get; set; }
        public bool Boolean { get; set; }
        public float Real { get; set; }
        public decimal Numeric { get; set; }
        public decimal Money { get; set; }
        public DateTime Date { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTimeOffset TimeStampTz { get; set; }
        public TimeSpan Time { get; set; }
        public TimeSpan Interval { get; set; }
        public string Char { get; set; }
        public string VarChar { get; set; }
        public string Text { get; set; }
        public Guid Uuid { get; set; }
        public byte[] Bytea { get; set; }
        public XDocument Xml { get; set; }
        public PhysicalAddress Macaddr { get; set; }
        public IPAddress Inet { get; set; }
        public (IPAddress, int) Cidr { get; set; }

        public int[] ArrayOfInt { get; set; }
        public string[] ArrayOfVarChar { get; set; }

        public static TypeModel GetSample()
        {
            return new TypeModel()
            {
                Real = 100.0f,
                Integer = 100000,
                Double = 100.0d,
                BigInt = 186116461616L,
                Boolean = true,
                Money = 2352323.35m,
                Date = new DateTime(1819, 5, 4),
                TimeStamp = new DateTime(1979, 5, 4, 3, 2, 1),
                TimeStampTz = new DateTimeOffset(1819, 5, 4, 3, 2, 1, TimeSpan.FromHours(3)),
                Time = new TimeSpan(3, 2, 1),
                Char = "Jerrycurl",
                VarChar = "Jerrycurl",
                Text = "Jerrycurl",
                SmallInt = 32000,
                Numeric = 100.03153433m,
                Uuid = new Guid("810f21f7-7e72-48d4-ba67-de5af147c668"),
                ArrayOfInt = new[] { 1, 2, 3, 4 },
                ArrayOfVarChar = new[] { "Jerry", "curl" },
                Bytea = Encoding.ASCII.GetBytes("Jerrycurl"),
                Xml = XDocument.Parse("<jerry>curl</jerry>"),
                Interval = new TimeSpan(-10, -9, -8),
                Cidr = (IPAddress.Parse("172.100.50.2"), 31),
                Inet = IPAddress.Parse("172.100.50.1"),
                Macaddr = PhysicalAddress.Parse("F0-E1-D2-C3-B4-A5"),
            };
        }
    }
}
