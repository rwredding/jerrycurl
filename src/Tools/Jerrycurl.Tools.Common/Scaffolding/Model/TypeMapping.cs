using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.Scaffolding.Model
{
    public class TypeMapping
    {
        public string DbName { get; }
        public string ClrName { get; }
        public bool IsValueType { get; }

        public TypeMapping(string dbName, string clrName, bool isValueType)
        {
            this.DbName = dbName;
            this.ClrName = clrName;
            this.IsValueType = isValueType;
        }
    }
}
