using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Mvc;
using Jerrycurl.Test.Project.Models;

namespace Jerrycurl.Test.Project.Accessors
{
    public class Runner : Accessor
    {
        private IList<TResult> QueryInternal<TModel, TResult>(Runnable<TModel, TResult> runner) => this.Query<TResult>(runner, queryName: "Query");
        private void CommandInternal<TModel, TResult>(Runnable<TModel, TResult> runner) => this.Execute(runner, commandName: "Command");

        public static IList<TResult> Query<TModel, TResult>(Runnable<TModel, TResult> runner) => new Runner().QueryInternal(runner);
        public static void Command<TModel, TResult>(Runnable<TModel, TResult> runner) => new Runner().CommandInternal(runner);
    }
}
