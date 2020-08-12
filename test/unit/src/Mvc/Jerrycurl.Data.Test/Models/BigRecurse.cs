using System.Collections.Generic;
using Jerrycurl.Data.Metadata.Annotations;

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
            [One]
            public One Parent { get; set; }
        }

        public class Many : BigRecurse
        {
            public IList<Many> Children { get; set; }
        }
    }
}
