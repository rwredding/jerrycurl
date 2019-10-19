using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Mvc.Metadata.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc.Test.Conventions.Models
{
    [Table("CrudItem")]
    public class CrudItem
    {
        [Id, Key]
        public int Id { get; set; }
        public int Counter { get; set; }
        public string String { get; set; }
    }
}
