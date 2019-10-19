using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Test.Models
{
    public abstract class BigRecurse
    {
        [Key("PK_Rec")]
        public int Id { get; set; }
        [Ref("PK_Rec")]
        public int? ParentId { get; set; }
        [Ref]
        public int? BigKey { get; set; }

        public class One : BigRecurse
        {
            [Many]
            public One Parent { get; set; }
        }

        public class Many : BigRecurse
        {
            public IList<Many> Children { get; set; }
        }
    }
}
