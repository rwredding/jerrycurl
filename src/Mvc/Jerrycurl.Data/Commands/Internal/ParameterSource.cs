using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class ParameterSource : IFieldSource
    {
        public IDbDataParameter AdoParameter { get; set; }
        public IParameter Parameter { get; set; }
        public bool HasSource => (this.Parameter != null);
        public bool HasTarget { get; set; }
        public bool HasChanged => this.HasTarget;

        public object Value
        {
            get
            {
                if (this.AdoParameter == null)
                    return DBNull.Value;

                return this.AdoParameter.Value;
            }
        }
    }
}
