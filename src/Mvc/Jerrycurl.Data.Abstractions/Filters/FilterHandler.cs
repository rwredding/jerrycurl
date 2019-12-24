using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Filters
{
    public class FilterHandler : IFilterHandler, IFilterAsyncHandler
    {
        #region " Synchronous "
        public virtual void OnCommandCreated(AdoCommandContext context) { }
        public virtual void OnCommandExecuted(AdoCommandContext context) { }
        public virtual void OnConnectionClosed(AdoConnectionContext context) { }
        public virtual void OnConnectionClosing(AdoConnectionContext context) { }
        public virtual void OnConnectionOpened(AdoConnectionContext context) { }
        public virtual void OnConnectionOpening(AdoConnectionContext context) { }
        public virtual void OnConnectionException(AdoConnectionContext context) { }
        public virtual void OnCommandException(AdoCommandContext context) { }
        public virtual void Dispose() { }

        #endregion

        #region " Asynchronous "

        public virtual Task OnConnectionOpeningAsync(AdoConnectionContext context) => Task.CompletedTask;
        public virtual Task OnConnectionOpenedAsync(AdoConnectionContext context) => Task.CompletedTask;
        public virtual Task OnConnectionClosingAsync(AdoConnectionContext context) => Task.CompletedTask;
        public virtual Task OnConnectionClosedAsync(AdoConnectionContext context) => Task.CompletedTask;
        public virtual Task OnConnectionExceptionAsync(AdoConnectionContext context) => Task.CompletedTask;
        public virtual Task OnCommandCreatedAsync(AdoCommandContext context) => Task.CompletedTask;
        public virtual Task OnCommandExecutedAsync(AdoCommandContext context) => Task.CompletedTask;
        public virtual Task OnCommandExceptionAsync(AdoCommandContext context) => Task.CompletedTask;

#if NETSTANDARD2_1
        public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
#endif

        #endregion
    }
}
