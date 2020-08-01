using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Sessions
{
    public interface ISessionOptions
    {
        IAsyncSession GetAsyncSession();
        ISyncSession GetSyncSession();
    }
}
