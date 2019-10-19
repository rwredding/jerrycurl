using Jerrycurl.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Filters
{
    public class TransactionFilter : IFilter
    {
        private readonly Func<Handler> factory;

        public TransactionFilter()
        {
            this.factory = () => new Handler(c => c.BeginTransaction());
        }

        public TransactionFilter(IsolationLevel isolationLevel)
        {
            this.factory = () => new Handler(c => c.BeginTransaction(isolationLevel));
        }

        public IFilterHandler GetHandler() => this.factory();

        private class Handler : FilterHandler
        {
            private readonly Func<IDbConnection, IDbTransaction> factory;
            private IDbTransaction transaction;
            private bool handled = false;

            public Handler(Func<IDbConnection, IDbTransaction> factory)
            {
                this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public override void OnConnectionOpened(AdoConnectionContext context)
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

            public override void OnConnectionException(AdoConnectionContext context)
            {
                this.Handle(() => this.transaction.Rollback());
            }

            public override void OnConnectionClosing(AdoConnectionContext context)
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
    }
}
