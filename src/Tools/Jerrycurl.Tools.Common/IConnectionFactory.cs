using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Jerrycurl.Tools
{
    public interface IConnectionFactory
    {
        DbConnection GetDbConnection();
    }
}
