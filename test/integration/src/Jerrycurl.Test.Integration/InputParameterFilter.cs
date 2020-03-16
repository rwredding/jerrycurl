using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Filters;
using System.Data;
using System.Linq;

namespace Jerrycurl.Test.Integration
{
    public class InputParameterFilter : FilterHandler, IFilter
    {
        public IFilterHandler GetHandler(IDbConnection connection) => this;
        public IFilterAsyncHandler GetAsyncHandler(IDbConnection connection) => null;

        public override void OnCommandCreated(FilterContext context)
        {
            foreach (IDbDataParameter parameter in context.Command.Parameters)
                parameter.Direction = ParameterDirection.Input;
        }
    }
}
