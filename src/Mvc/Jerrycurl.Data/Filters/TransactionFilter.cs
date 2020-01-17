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

#if !NETSTANDARD2_0
        public IFilterAsyncHandler GetAsyncHandler(IDbConnection connection)
        {
            if (connection is DbConnection)
                return this.asyncFactory();

            return null;
        }
#else
        public IFilterAsyncHandler GetAsyncHandler(IDbConnection connection) => null;
#endif

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
                    this.transaction.Rollback();

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
#if !NETSTANDARD2_0
            private DbTransaction transaction;
            private bool handled = false;
#endif

            public AsyncHandler(Func<IDbConnection, IDbTransaction> factory)
            {
                this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

#if !NETSTANDARD2_0
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

            public override async Task OnExceptionAsync(FilterContext context)
            {
                if (!this.handled)
                    await this.transaction.RollbackAsync();

                this.handled = true;
            }

            public override async Task OnConnectionClosingAsync(FilterContext context)
            {
                if (!this.handled)
                    await this.transaction.CommitAsync();

                this.handled = true;
            }

            private async Task HandleAsync(Func<Task> action)
            {
                if (!this.handled)
                    await action();

                this.handled = true;
            }

            public override async ValueTask DisposeAsync()
            {
                await this.transaction.DisposeAsync();

                this.transaction = null;
            }

#endif
        }
    }
}
