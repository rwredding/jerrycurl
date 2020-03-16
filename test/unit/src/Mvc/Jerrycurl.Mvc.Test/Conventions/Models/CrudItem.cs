using Jerrycurl.Data.Metadata.Annotations;

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
