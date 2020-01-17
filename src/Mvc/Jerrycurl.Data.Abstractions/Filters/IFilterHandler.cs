using System;

namespace Jerrycurl.Data.Filters
{
    public interface IFilterHandler : IDisposable
    {
        void OnConnectionOpening(FilterContext context);
        void OnConnectionOpened(FilterContext context);
        void OnConnectionClosing(FilterContext context);
        void OnConnectionClosed(FilterContext context);
        void OnCommandCreated(FilterContext context);
        void OnCommandExecuted(FilterContext context);
        void OnException(FilterContext context);
    }
}
