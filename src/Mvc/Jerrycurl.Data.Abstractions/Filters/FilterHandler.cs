using System.Threading.Tasks;

namespace Jerrycurl.Data.Filters
{
    public class FilterHandler : IFilterHandler, IFilterAsyncHandler
    {
        #region " Synchronous "
        public virtual void OnCommandCreated(FilterContext context) { }
        public virtual void OnCommandExecuted(FilterContext context) { }
        public virtual void OnConnectionClosed(FilterContext context) { }
        public virtual void OnConnectionClosing(FilterContext context) { }
        public virtual void OnConnectionOpened(FilterContext context) { }
        public virtual void OnConnectionOpening(FilterContext context) { }
        public virtual void OnConnectionException(FilterContext context) { }
        public virtual void OnException(FilterContext context) { }
        public virtual void Dispose() { }

        #endregion

        #region " Asynchronous "

        public virtual Task OnConnectionOpeningAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnConnectionOpenedAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnConnectionClosingAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnConnectionClosedAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnCommandCreatedAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnCommandExecutedAsync(FilterContext context) => Task.CompletedTask;
        public virtual Task OnExceptionAsync(FilterContext context) => Task.CompletedTask;
        public virtual ValueTask DisposeAsync() => default;

        #endregion
    }
}
