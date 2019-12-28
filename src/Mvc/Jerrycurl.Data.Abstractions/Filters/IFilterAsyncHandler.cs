using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Filters
{
    public interface IFilterAsyncHandler
#if NETSTANDARD2_1
        : IAsyncDisposable
#endif
    {
        Task OnConnectionOpeningAsync(FilterContext context);
        Task OnConnectionOpenedAsync(FilterContext context);
        Task OnConnectionClosingAsync(FilterContext context);
        Task OnConnectionClosedAsync(FilterContext context);
        Task OnConnectionExceptionAsync(FilterContext context);
        Task OnCommandCreatedAsync(FilterContext context);
        Task OnCommandExecutedAsync(FilterContext context);
        Task OnCommandExceptionAsync(FilterContext context);
    }
}
