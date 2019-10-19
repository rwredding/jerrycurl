using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class FieldData
    {
        private readonly IField field;
        private object value;
        private IDbDataParameter parameter;
        private ColumnIdentity columnInfo;

        public MetadataIdentity Attribute { get; }

        public FieldData(IField field)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
            this.Attribute = field.Identity.Metadata;
        }

        public void SetValue(IDbDataParameter adoParameter)
        {
            this.parameter = adoParameter;
            this.columnInfo = null;
        }

        public void SetCellInfo(ColumnIdentity cellInfo)
        {
            this.columnInfo = cellInfo;
            this.parameter = null;
        }

        public void SetValue(object value)
        {
            this.value = value;
            this.parameter = null;
        }

        public object GetValue()
        {
            if (this.parameter != null)
                return this.parameter.Value;

            return this.value;
        }

        public void Bind()
        {
            Action<IField, object> binder = FuncCache.GetFieldBinder(this.Attribute, this.columnInfo);

            binder(this.field, this.GetValue());
        }
    }
}
