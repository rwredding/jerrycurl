using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Filters
{
    public class TransactionFilter : IFilter
    {
        private readonly Func<Handler> factory;
        private readonly Func<AsyncHandler> asyncFactory;

        public TransactionFilter()
        {
            this.factory = () => new Handler(c => c.BeginTransaction());
            this.asyncFactory = () => new AsyncHandler(c => c.BeginTransaction());
        }

        public TransactionFilter(IsolationLevel isolationLevel)
        {
            this.factory = () => new Handler(c => c.BeginTransaction(isolationLevel));
            this.asyncFactory = () => new AsyncHandler(c => c.BeginTransaction(isolationLevel));
        }

        public IFilterHandler GetHandler(IDbConnection connection) => this.factory();
        public IFilterAsyncHandler GetAsyncHandler(IDbConnection connection) => this.asyncFactory();

        private class Handler : FilterHandler
        {
            private readonly Func<IDbConnection, IDbTransaction> factory;
            private IDbTransaction transaction;
            private bool handled = false;

            public Handler(Func<IDbConnection, IDbTransaction> factory)
            {
                this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public override void OnConnectionOpened(FilterContext context)
            {
                this.transaction = this.factory(context.Connection);
            }

            public override void OnCommandCreated(FilterContext context)
            {
                context.Command.Transaction = this.transaction;
            }

            public override void OnException(FilterContext context)
            {
                if (!this.handled)
                {
                    try
                    {
                        this.transaction.Rollback();
                    }
                    catch { }
                }

                this.handled = true;
            }

            public override void OnConnectionClosing(FilterContext context)
            {
                if (!this.handled)
                    this.transaction.Commit();

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
            private readonly Func<IDbConnection, IDbTransaction> factory;
            private DbTransaction transaction;
            private bool handled = false;

            public AsyncHandler(Func<IDbConnection, IDbTransaction> factory)
            {
                this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public override Task OnConnectionOpenedAsync(FilterContext context)
            {
                this.transaction = this.factory(context.Connection) as DbTransaction;

                return Task.CompletedTask;
            }

            public override Task OnCommandCreatedAsync(FilterContext context)
            {
                context.Command.Transaction = this.transaction;

                return Task.CompletedTask;
            }

            #if !NETSTANDARD2_0
            public override async Task OnExceptionAsync(FilterContext context)
            {
                if (!this.handled)
                {
                    try
                    {
                        await this.transaction.RollbackAsync().ConfigureAwait(false);
                    }
                    catch { }
                }

                this.handled = true;
            }

            public override async Task OnConnectionClosingAsync(FilterContext context)
            {
                if (!this.handled)
                    await this.transaction.CommitAsync().ConfigureAwait(false);

                this.handled = true;
            }

            public override async ValueTask DisposeAsync()
            {
                await this.transaction.DisposeAsync().ConfigureAwait(false);

                this.transaction = null;
            }
#else
            public override Task OnExceptionAsync(FilterContext context)
            {
                if (!this.handled)
                {
                    try
                    {
                        this.transaction.Rollback();
                    }
                    catch { }
                }

                this.handled = true;

                return Task.CompletedTask;
            }

            public override Task OnConnectionClosingAsync(FilterContext context)
            {
                if (!this.handled)
                    this.transaction.Commit();

                this.handled = true;

                return Task.CompletedTask;
            }

            public override ValueTask DisposeAsync()
            {
                this.transaction.Dispose();
                this.transaction = null;

                return default;
            }
#endif
        }
    }
}