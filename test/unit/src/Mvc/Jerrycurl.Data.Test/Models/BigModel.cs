using System.Collections.Generic;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Test.Models
{
    public class BigModel
    {
        [Key]
        public int BigKey { get; set; }
        [Key(IsPrimary = false)]
        public int? NonPrimaryKey { get; set; }
        public int Value { get; set; }
        public string Value2 { get; set; }
        public int ReadOnly { get; }

        public IList<SubModel> OneToMany { get; set; }
        public IList<BigRecurse.Many> OneToManySelf { get; set; }
        public SubModel OneToOne { get; set; }
        [One]
        public SubModel OneToManyAsOne { get; set; }
        public Many<SubModel> ManyType { get; set; }
        
        public class SubModel
        {
            [Key]
            public int SubKey { get; set; }
            [Ref]
            public int BigKey { get; set; }
            [Ref]
            public int? NonPrimaryKey { get; set; }
            public int Value { get; set; }
        }
    }
}
