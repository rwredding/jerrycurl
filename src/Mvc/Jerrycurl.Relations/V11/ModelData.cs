using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11
{
    internal class ModelData : IFieldData
    {
        public object Relation => null;
        public int Index => 0;
        public object Parent => null;
        public object Value { get; }

        public ModelData(object value)
        {
            this.Value = value;
        }
    }
}
