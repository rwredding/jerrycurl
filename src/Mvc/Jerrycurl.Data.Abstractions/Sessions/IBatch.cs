using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Sessions
{
    public interface IBatch
    {
        void Build(IDbCommand adoCommand);
    }
}
