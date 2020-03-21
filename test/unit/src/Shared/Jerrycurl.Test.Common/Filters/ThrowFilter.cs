using System;
using System.Threading.Tasks;
using Jerrycurl.Data.Filters;

namespace Jerrycurl.Test.Filters
{
    public class ThrowFilter : FilterHandler
    {
        public override void OnCommandExecuted(FilterContext context) => throw new Exception();
        public override Task OnCommandExecutedAsync(FilterContext context) => Task.FromException(new Exception());
    }
}
