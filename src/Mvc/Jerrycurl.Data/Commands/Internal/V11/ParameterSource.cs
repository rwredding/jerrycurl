using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Commands.Internal.V11
{
    internal class ParameterSource : IFieldSource
    {
        public IDbDataParameter AdoParameter { get; set; }
        public IParameter Parameter { get; set; }
        public bool HasChanged => this.IsOutputParameter();

        public object Value
        {
            get
            {
                if (this.AdoParameter == null)
                    return DBNull.Value;

                return this.AdoParameter.Value;
            }
            set
            {
                if (this.AdoParameter != null)
                    this.AdoParameter.Value = value;
            }
        }

        private bool IsOutputParameter()
        {
            if (this.AdoParameter == null)
                return false;
            else if (this.AdoParameter.Direction == ParameterDirection.InputOutput)
                return true;
            else if (this.AdoParameter.Direction == ParameterDirection.Output)
                return true;

            return false;
        }
    }
}
