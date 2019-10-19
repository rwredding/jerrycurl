using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Filters
{
    public interface IFilterHandler : IDisposable
    {
        void OnConnectionOpening(AdoConnectionContext context);
        void OnConnectionOpened(AdoConnectionContext context);
        void OnConnectionClosing(AdoConnectionContext context);
        void OnConnectionClosed(AdoConnectionContext context);
        void OnConnectionException(AdoConnectionContext context);
        void OnCommandCreated(AdoCommandContext context);
        void OnCommandExecuted(AdoCommandContext context);
        void OnCommandException(AdoCommandContext context);
    }
}
