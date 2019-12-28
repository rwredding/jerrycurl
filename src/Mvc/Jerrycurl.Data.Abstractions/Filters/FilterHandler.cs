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
        public virtual void OnConnectionClosed(FilterContext context) { }
        public virtual void OnConnectionClosing(FilterContext context) { }
        public virtual void OnConnectionOpened(FilterContext context) { }
        public virtual void OnConnectionOpening(FilterContext context) { }
        public virtual void OnConnectionException(FilterContext context) { }
        public virtual void OnCommandException(AdoCommandContext context) { }
        public virtual void Dispose() { }

        #endregion

        #region " Asynchronous "

        public virtual Task OnConnectionOpeningAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnConnectionOpenedAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnConnectionClosingAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnConnectionClosedAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnConnectionExceptionAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnCommandCreatedAsync(AdoCommandContext context) => Task.CompletedTask;
        public virtual Task OnCommandExecutedAsync(AdoCommandContext context) => Task.CompletedTask;
        public virtual Task OnCommandExceptionAsync(AdoCommandContext context) => Task.CompletedTask;

#if NETSTANDARD2_1
        public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
#endif

        #endregion
    }
}
