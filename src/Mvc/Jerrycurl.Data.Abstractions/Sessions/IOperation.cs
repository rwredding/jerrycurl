using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Jerrycurl.Data.Sessions
{
    public interface IOperation
    {
        object Source { get; }

        void Build(IDbCommand adoCommand);
    }
}
