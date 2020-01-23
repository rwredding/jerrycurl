using System;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;

namespace Jerrycurl.Data.Filters
{
    public class TransactionScopeFilter : IFilter
    {
        private readonly Func<Handler> handler;
        private readonly Func<AsyncHandler> asyncHandler;

        public TransactionScopeFilter()
            : this(() => new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {

        }

        public TransactionScopeFilter(TransactionScopeOption scopeOption)
            : this(() => new TransactionScope(scopeOption, TransactionScopeAsyncFlowOption.Enabled))
        {

        }

        public TransactionScopeFilter(Func<TransactionScope> scopeFactory)
        {
            if (scopeFactory == null)
                throw new ArgumentNullException(nameof(scopeFactory));

            this.handler = () => new Handler(scopeFactory);
            this.asyncHandler = () => new AsyncHandler(scopeFactory);
        }

        public IFilterHandler GetHandler(IDbConnection connection) => this.handler();
        public IFilterAsyncHandler GetAsyncHandler(IDbConnection connection) => this.asyncHandler();

        private class Handler : FilterHandler
        {
            private TransactionScope transaction;
            private bool handled = false;

            public Handler(Func<TransactionScope> factory)
            {
                this.transaction = factory();
            }

            public override void OnException(FilterContext context)
            {
                if (!this.handled)
                    this.handled = true;
            }

            public override void OnConnectionClosed(FilterContext context)
            {
                if (!this.handled)
                    this.transaction.Complete();

                this.handled = true;
            }

            public override void Dispose()
            {
                this.transaction.Dispose();
                this.transaction = null;
            }
        }

        private class AsyncHandler : FilterHandler
        {
            private TransactionScope transaction;
            private bool handled = false;

            public AsyncHandler(Func<TransactionScope> factory)
            {
                this.transaction = factory();
            }

            public override Task OnExceptionAsync(FilterContext context)
            {
                if (!this.handled)
                    this.handled = true;

                return Task.CompletedTask;
            }

            public override Task OnConnectionClosedAsync(FilterContext context)
            {
                if (!this.handled)
                    this.transaction.Complete();

                this.handled = true;

                return Task.CompletedTask;
            }

            public override ValueTask DisposeAsync()
            {
                this.transaction.Dispose();
                this.transaction = null;

                return default;
            }
        }
    }
}
