using Jerrycurl.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
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

        public IFilterHandler GetHandler() => this.factory();
        public IFilterAsyncHandler GetAsyncHandler() => this.asyncFactory();

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

            public override void OnCommandCreated(AdoCommandContext context)
            {
                context.Command.Transaction = this.transaction;
            }

            public override void OnCommandException(AdoCommandContext context)
            {
                this.Handle(() => this.transaction.Rollback());
            }

            public override void OnConnectionException(FilterContext context)
            {
                this.Handle(() => this.transaction.Rollback());
            }

            public override void OnConnectionClosing(FilterContext context)
            {
                this.Handle(() => this.transaction.Commit());
            }

            private void Handle(Action action)
            {
                if (!this.handled)
                    action();

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

                if (this.transaction == null)
                    throw new InvalidOperationException("Async not available.");

                return Task.CompletedTask;
            }

            public override Task OnCommandCreatedAsync(FilterContext context)
            {
                context.Command.Transaction = this.transaction;

                return Task.CompletedTask;
            }

            public override async Task OnCommandExceptionAsync(FilterContext context)
            {
                await this.HandleAsync(() => this.transaction.RollbackAsync());
            }

            public override async Task OnConnectionExceptionAsync(FilterContext context)
            {
                await this.HandleAsync(() => this.transaction.RollbackAsync());
            }

            public override async Task OnConnectionClosingAsync(FilterContext context)
            {
                await this.HandleAsync(() => this.transaction.CommitAsync());
            }

            private async Task HandleAsync(Func<Task> action)
            {
                if (!this.handled)
                    await action();

                this.handled = true;
            }

            public override void Dispose()
            {
                this.transaction.Dispose();
                this.transaction = null;
            }
        }
    }
}
