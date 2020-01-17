using System.Collections.Generic;

namespace Jerrycurl.Tools.Scaffolding.Model
{
    public class ColumnModel
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public bool Ignore { get; set; }

        public IList<KeyModel> Keys { get; set; } = new List<KeyModel>();
        public IList<ReferenceModel> References { get; set; } = new List<ReferenceModel>();
    }
}
