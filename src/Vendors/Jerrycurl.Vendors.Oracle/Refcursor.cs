using System;
using System.Data;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations;
using Oracle.ManagedDataAccess.Client;

namespace Jerrycurl.Vendors.Oracle
{
    public class Refcursor : IParameter
    {
        public string Name { get; }

        public Refcursor(string name)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public void Build(IDbDataParameter adoParameter)
        {
            if (adoParameter is OracleParameter op)
            {
                op.Direction = ParameterDirection.ReturnValue;
                op.OracleDbType = OracleDbType.RefCursor;
            }
            else
                throw new InvalidOperationException("Refcursors are only available for parameters of type OracleParameter.");
        }

        IField IParameter.Field => null;
    }
}
