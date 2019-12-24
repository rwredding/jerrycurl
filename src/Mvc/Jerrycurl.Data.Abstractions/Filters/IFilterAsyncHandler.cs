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
        Task OnConnectionOpeningAsync(AdoConnectionContext context);
        Task OnConnectionOpenedAsync(AdoConnectionContext context);
        Task OnConnectionClosingAsync(AdoConnectionContext context);
        Task OnConnectionClosedAsync(AdoConnectionContext context);
        Task OnConnectionExceptionAsync(AdoConnectionContext context);
        Task OnCommandCreatedAsync(AdoCommandContext context);
        Task OnCommandExecutedAsync(AdoCommandContext context);
        Task OnCommandExceptionAsync(AdoCommandContext context);
    }
}
