using Jerrycurl.Relations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class FieldMap : IEnumerable<FieldData>
    {
        private readonly Dictionary<IField, FieldData> fieldMap = new Dictionary<IField, FieldData>();
        private readonly List<FieldData> fieldList = new List<FieldData>();

        public FieldData Get(IField field)
        {
            if (this.fieldMap.TryGetValue(field, out FieldData fieldData))
                return fieldData;

            return null;
        }

        public FieldData Add(IField field)
        {
            FieldData fieldData = new FieldData(field);

            this.fieldMap[field] = fieldData;
            this.fieldList.Add(fieldData);

            return fieldData;
        }

        public IEnumerator<FieldData> GetEnumerator() => this.fieldList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
