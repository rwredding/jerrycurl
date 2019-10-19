using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jerrycurl.Data.Metadata;
using Oracle.ManagedDataAccess.Client;

namespace Jerrycurl.Vendors.Oracle
{
    public class RefcursorContract : IBindingParameterContract
    {
        public BindingParameterWriter Write => this.GetWriteParameterProxy;
        public BindingParameterConverter Convert => o => DBNull.Value;

        private void GetWriteParameterProxy(IBindingParameterInfo paramInfo)
        {
            if (paramInfo.Parameter is OracleParameter op)
            {
                op.Direction = ParameterDirection.ReturnValue;
                op.OracleDbType = OracleDbType.RefCursor;
            }
            else
                throw new InvalidOperationException("Refcursors are only available for parameters of type OracleParameter.");
        }
    }
}
