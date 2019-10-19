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

        public IFilterHandler GetHandler() => this.handler();

        private class Handler : FilterHandler
        {
            private readonly Func<TransactionScope> factory;
            private TransactionScope transaction;

            public Handler(Func<TransactionScope> factory)
            {
                this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public override void OnConnectionOpening(AdoConnectionContext context)
            {
                this.transaction = this.factory();
            }

            public override void OnConnectionClosing(AdoConnectionContext context)
            {
                this.transaction?.Complete();
            }

            public override void OnCommandException(AdoCommandContext context)
            {
                this.transaction?.Dispose();
                this.transaction = null;
            }

            public override void OnConnectionException(AdoConnectionContext context)
            {
                this.transaction?.Dispose();
                this.transaction = null;
            }

            public override void Dispose()
            {
                this.transaction?.Dispose();
            }
        }
    }
}
