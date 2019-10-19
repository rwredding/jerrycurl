using Jerrycurl.Data.Filters;
using System;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Jerrycurl.Mvc
{
    public static class TransactionExtensions
    {
        public static void UseTransaction(this SqlOptions options) => options.Filters.Add(new TransactionFilter());
        public static void UseTransaction(this SqlOptions options, IsolationLevel isolationLevel) => options.Filters.Add(new TransactionFilter(isolationLevel));

        public static void UseTransactionScope(this SqlOptions options) => options.Filters.Add(new TransactionScopeFilter());
        public static void UseTransactionScope(this SqlOptions options, TransactionScopeOption scopeOption) => options.Filters.Add(new TransactionScopeFilter(scopeOption));
        public static void UseTransactionScope(this SqlOptions options, Func<TransactionScope> scopeFactory) => options.Filters.Add(new TransactionScopeFilter(scopeFactory));
    }
}
