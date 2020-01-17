using System;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Filters
{
    public partial interface IFilterAsyncHandler : IAsyncDisposable, IDisposable
    {
        Task OnConnectionOpeningAsync(FilterContext context);
        Task OnConnectionOpenedAsync(FilterContext context);
        Task OnConnectionClosingAsync(FilterContext context);
        Task OnConnectionClosedAsync(FilterContext context);
        Task OnCommandCreatedAsync(FilterContext context);
        Task OnCommandExecutedAsync(FilterContext context);
        Task OnExceptionAsync(FilterContext context);
    }
}
