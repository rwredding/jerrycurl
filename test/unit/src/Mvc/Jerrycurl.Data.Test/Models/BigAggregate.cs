using System.Collections.Generic;
using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Data.Test.Models
{
    public class BigAggregate
    {
        public int Scalar { get; set; }
        public BigModel One { get; set; }
        public BigModel None { get; set; }
        public IList<BigModel> Many { get; set; }

        public BigModel NotUsedOne { get; set; }
        public IList<BigModel> NotUsedMany { get; set; }
    }
}
