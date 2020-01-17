using System.Data;
using Jerrycurl.Data.Filters;
using Oracle.ManagedDataAccess.Client;

namespace Jerrycurl.Vendors.Oracle.Filters
{
    internal class OracleLongFilter : FilterHandler, IFilter
    {
        public IFilterAsyncHandler GetAsyncHandler(IDbConnection connection) => null;
        public IFilterHandler GetHandler(IDbConnection connection) => this;

        public override void OnCommandCreated(FilterContext context)
        {
            base.OnCommandCreated(context);

            if (context.Command is OracleCommand oracleCommand)
                oracleCommand.InitialLONGFetchSize = -1;
        }
    }
}
