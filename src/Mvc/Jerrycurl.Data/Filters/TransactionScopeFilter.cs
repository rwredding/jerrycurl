using Jerrycurl.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace Jerrycurl.Data.Filters
{
    public class TransactionScopeFilter : IFilter
    {
        private readonly Func<Handler> handler;

        public TransactionScopeFilter()
            : this(() => new TransactionScope())
        {

        }

        public TransactionScopeFilter(TransactionScopeOption scopeOption)
            : this(() => new TransactionScope(scopeOption))
        {

        }

        public TransactionScopeFilter(Func<TransactionScope> scopeFactory)
        {
            if (scopeFactory == null)
                throw new ArgumentNullException(nameof(scopeFactory));

            this.handler = () => new Handler(scopeFactory);
        }

        public IFilterHandler GetHandler(IDbConnection connection) => this.handler();
        public IFilterAsyncHandler GetAsyncHandler(IDbConnection connection) => null;

        private class Handler : FilterHandler
        {
            private readonly Func<TransactionScope> factory;
            private TransactionScope transaction;
            private bool handled = false;

            public Handler(Func<TransactionScope> factory)
            {
                this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public override void OnConnectionOpening(FilterContext context)
            {
                this.transaction = this.factory();
            }

            public override void OnConnectionClosed(FilterContext context)
            {
                if (!this.handled)
                    this.transaction.Complete();

                this.handled = true;
            }

            public override void Dispose()
            {
                this.transaction?.Dispose();
                this.transaction = null;
            }
        }
    }
}
