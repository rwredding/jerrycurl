using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Jerrycurl.Data
{
    public interface IAdoCommandBuilder
    {
        void Build(IDbCommand adoCommand);
    }
}
