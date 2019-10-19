using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Filters;
using Oracle.ManagedDataAccess.Client;

namespace Jerrycurl.Vendors.Oracle.Filters
{
    class OracleLongFilter : FilterHandler, IFilter
    {
        public IFilterHandler GetHandler() => this;

        public override void OnCommandCreated(AdoCommandContext context)
        {
            base.OnCommandCreated(context);

            if (context.Command is OracleCommand oracleCommand)
                oracleCommand.InitialLONGFetchSize = -1;
        }
    }
}
