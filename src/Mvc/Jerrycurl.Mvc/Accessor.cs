using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;

namespace Jerrycurl.Mvc
{
    /// <summary>
    /// Provides a base class for executing database queries and commands from Razor SQL-enabled pages and materializing their results.
    /// </summary>
    public abstract class Accessor
    {
        public AccessorContext Context { get; set; }

        /// <summary>
        /// Executes an asynchronous Razor SQL query with a specified model and returns a single, continuous, unbuffered list from its result sets.
        /// </summary>
        /// <typeparam name="TItem">The resulting type of each item in the list.</typeparam>
        /// <param name="model">The concrete model to initialize to the Razor page with.</param>
        /// <param name="configure">A method for configuring query options.</param>
        /// <param name="queryName">The query name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An unbuffered list of <typeparamref name="TItem"/> instances.</returns>
        protected IAsyncEnumerable<TItem> EnumerateAsync<TItem>(object model = null, Action<SqlOptions> configure = null, [CallerMemberName]string queryName = null, CancellationToken cancellationToken = default)
        {
            IProcResult result = this.ExecuteAndGetResult(queryName, model, new ProcArgs()
            {
                ModelType = model?.GetType() ?? typeof(object),
                ResultType = typeof(IList<TItem>),
            });

            SqlOptions options = this.GetQueryOptions(result.Domain);
            configure?.Invoke(options);

            ISqlSerializer<QueryData> serializer = result.Buffer as ISqlSerializer<QueryData>;
            IEnumerable<QueryData> queries = serializer?.Serialize(options);

            QueryOptions queryOptions = new QueryOptions()
            {
                ConnectionFactory = result.Domain.ConnectionFactory,
                Schemas = result.Domain.Schemas,
                Filters = options.Filters,
            };

            QueryHandler handler = new QueryHandler(queryOptions);

            return handler.EnumerateAsync<TItem>(queries, cancellationToken);
        }

        /// <summary>
        /// Executes an asynchronous Razor SQL query with a specified model and returns an unbuffered list of <see cref="QueryReader"/> instances each of which can enumerate items in its corresponding result set.
        /// </summary>
        /// <param name="model">The concrete model to initialize to the Razor page with.</param>
        /// <param name="configure">A method for configuring query options.</param>
        /// <param name="queryName">The query name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An enumerable providing a <see cref="QueryReader"/> instance for every data set in the result.</returns>
        protected IAsyncEnumerable<QueryReader> EnumerateAsync(object model = null, Action<SqlOptions> configure = null, [CallerMemberName]string queryName = null, CancellationToken cancellationToken = default)
        {
            IProcResult result = this.ExecuteAndGetResult(queryName, model, new ProcArgs()
            {
                ModelType = model?.GetType() ?? typeof(object),
                ResultType = typeof(IList<object>),
            });

            SqlOptions options = this.GetQueryOptions(result.Domain);
            configure?.Invoke(options);

            ISqlSerializer<QueryData> serializer = result.Buffer as ISqlSerializer<QueryData>;
            IEnumerable<QueryData> queries = serializer?.Serialize(options);

            QueryOptions queryOptions = new QueryOptions()
            {
                ConnectionFactory = result.Domain.ConnectionFactory,
                Schemas = result.Domain.Schemas,
                Filters = options.Filters,
            };

            QueryHandler handler = new QueryHandler(queryOptions);

            return handler.EnumerateAsync(queries, cancellationToken);
        }

        /// <summary>
        /// Executes a Razor SQL query with a specified model and returns a single, continuous, unbuffered list from its result sets.
        /// </summary>
        /// <typeparam name="TItem">The resulting type of each item in the list.</typeparam>
        /// <param name="model">The concrete model to initialize to the Razor page with.</param>
        /// <param name="configure">A method for configuring query options.</param>
        /// <param name="queryName">The query name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <returns>An unbuffered list of <typeparamref name="TItem"/> items.</returns>
        protected IEnumerable<TItem> Enumerate<TItem>(object model = null, Action<SqlOptions> configure = null, [CallerMemberName]string queryName = null)
        {
            IProcResult result = this.ExecuteAndGetResult(queryName, model, new ProcArgs()
            {
                ModelType = model?.GetType() ?? typeof(object),
                ResultType = typeof(IList<TItem>),
            });

            SqlOptions options = this.GetQueryOptions(result.Domain);
            configure?.Invoke(options);

            ISqlSerializer<QueryData> serializer = result.Buffer as ISqlSerializer<QueryData>;
            IEnumerable<QueryData> queries = serializer?.Serialize(options);

            QueryOptions queryOptions = new QueryOptions()
            {
                ConnectionFactory = result.Domain.ConnectionFactory,
                Schemas = result.Domain.Schemas,
                Filters = options.Filters,
            };

            QueryHandler handler = new QueryHandler(queryOptions);

            return handler.Enumerate<TItem>(queries);
        }

        /// <summary>
        /// Executes a Razor SQL query with a specified model and returns an unbuffered list of <see cref="QueryReader"/> instances each of which can enumerate items in its corresponding result set.
        /// </summary>
        /// <param name="model">A concrete model containing parameter values for the query.</param>
        /// <param name="configure">A method for configuring query options.</param>
        /// <param name="queryName">The query name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <returns>An enumerable providing a <see cref="QueryReader"/> instance for every data set in the result.</returns>
        protected IEnumerable<QueryReader> Enumerate(object model = null, Action<SqlOptions> configure = null, [CallerMemberName]string queryName = null)
        {
            IProcResult result = this.ExecuteAndGetResult(queryName, model, new ProcArgs()
            {
                ModelType = model?.GetType() ?? typeof(object),
                ResultType = typeof(IList<object>),
            });

            SqlOptions options = this.GetQueryOptions(result.Domain);
            configure?.Invoke(options);

            ISqlSerializer<QueryData> serializer = result.Buffer as ISqlSerializer<QueryData>;
            IEnumerable<QueryData> queries = serializer?.Serialize(options);

            QueryOptions queryOptions = new QueryOptions()
            {
                ConnectionFactory = result.Domain.ConnectionFactory,
                Schemas = result.Domain.Schemas,
                Filters = options.Filters,
            };

            QueryHandler handler = new QueryHandler(queryOptions);

            return handler.Enumerate(queries);
        }

        /// <summary>
        /// Calls <see cref="Query{TItem}(object, Action{SqlOptions}, string)"/> and returns its first result, if any.
        /// </summary>
        /// <typeparam name="TItem">The resulting type of each item in the list.</typeparam>
        /// <param name="model">A concrete model containing parameter values for the query.</param>
        /// <param name="configure">A method for configuring query options.</param>
        /// <param name="queryName">The query name to locate the Razor page by.</param>
        /// <returns>An buffered list of <typeparamref name="TItem"/> items.</returns>
        protected TItem One<TItem>(object model = null, Action<SqlOptions> configure = null, [CallerMemberName]string queryName = null)
            => this.Query<TItem>(model, configure, queryName).SingleOrDefault();

        /// <summary>
        /// Executes a Razor SQL query with a specified model and returns a single, buffered list from the product of its result sets.
        /// </summary>
        /// <typeparam name="TItem">The resulting type of each item in the list.</typeparam>
        /// <param name="model">A concrete model containing parameter values for the query.</param>
        /// <param name="configure">A method for configuring query options.</param>
        /// <param name="queryName">The query name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <returns>An buffered list of <typeparamref name="TItem"/> items.</returns>
        protected IList<TItem> Query<TItem>(object model = null, Action<SqlOptions> configure = null, [CallerMemberName]string queryName = null)
        {
            IProcResult result = this.ExecuteAndGetResult(queryName, model, new ProcArgs()
            {
                ModelType = model?.GetType() ?? typeof(object),
                ResultType = typeof(IList<TItem>),
            });

            SqlOptions options = this.GetQueryOptions(result.Domain);
            configure?.Invoke(options);

            ISqlSerializer<QueryData> serializer = result.Buffer as ISqlSerializer<QueryData>;
            IEnumerable<QueryData> queries = serializer?.Serialize(options);

            QueryOptions queryOptions = new QueryOptions()
            {
                ConnectionFactory = result.Domain.ConnectionFactory,
                Schemas = result.Domain.Schemas,
                Filters = options.Filters,
            };

            QueryHandler handler = new QueryHandler(queryOptions);

            return handler.List<TItem>(queries);
        }

        /// <summary>
        /// Calls <see cref="QueryAsync{TItem}(object, Action{SqlOptions}, string, CancellationToken)"/> and returns its first result, if any.
        /// </summary>
        /// <typeparam name="TItem">The resulting type of each item in the list.</typeparam>
        /// <param name="model">A concrete model containing parameter values for the query.</param>
        /// <param name="configure">A method for configuring query options.</param>
        /// <param name="queryName">The query name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An buffered list of <typeparamref name="TItem"/> items.</returns>
        protected async Task<TItem> OneAsync<TItem>(object model = null, Action<SqlOptions> configure = null, [CallerMemberName]string queryName = null, CancellationToken cancellationToken = default)
            => (await this.QueryAsync<TItem>(model, configure, queryName, cancellationToken).ConfigureAwait(false)).SingleOrDefault();

        /// <summary>
        /// Executes an asynchronous Razor SQL query with a specified model and returns a single, buffered list from the product of its result sets.
        /// </summary>
        /// <typeparam name="TItem">The resulting type of each item in the list.</typeparam>
        /// <param name="model">A concrete model containing parameter values for the query.</param>
        /// <param name="configure">A method for configuring query options.</param>
        /// <param name="queryName">The query name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An buffered list of <typeparamref name="TItem"/> items.</returns>
        protected async Task<IList<TItem>> QueryAsync<TItem>(object model = null, Action<SqlOptions> configure = null, [CallerMemberName]string queryName = null, CancellationToken cancellationToken = default)
        {
            IProcResult result = this.ExecuteAndGetResult(queryName, model, new ProcArgs()
            {
                ModelType = model?.GetType() ?? typeof(object),
                ResultType = typeof(IList<TItem>),
            });

            SqlOptions options = this.GetQueryOptions(result.Domain);
            configure?.Invoke(options);

            ISqlSerializer<QueryData> serializer = result.Buffer as ISqlSerializer<QueryData>;
            IEnumerable<QueryData> queries = serializer?.Serialize(options);

            QueryOptions queryOptions = new QueryOptions()
            {
                ConnectionFactory = result.Domain.ConnectionFactory,
                Schemas = result.Domain.Schemas,
                Filters = options.Filters,
            };

            QueryHandler handler = new QueryHandler(queryOptions);

            return await handler.ListAsync<TItem>(queries, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a Razor SQL command with the specified model and updates the model from its resulting data set.
        /// </summary>
        /// <param name="model">A concrete model containing parameter values for the command.</param>
        /// <param name="configure">A method for configuring command options.</param>
        /// <param name="commandName">The command name to locate the Razor page by. Defaults to the name of the calling method.</param>
        protected void Execute(object model = default, Action<SqlOptions> configure = null, [CallerMemberName]string commandName = null) => this.Execute<object>(model, configure, commandName);

        /// <summary>
        /// Executes a Razor SQL command with the specified model and updates the model from its resulting data set.
        /// </summary>
        /// <typeparam name="TModel">Type of the concrete parameter model.</typeparam>
        /// <param name="model">A concrete model containing parameter values for the command.</param>
        /// <param name="configure">A method for configuring command options.</param>
        /// <param name="commandName">The command name to locate the Razor page by. Defaults to the name of the calling method.</param>
        protected void Execute<TModel>(TModel model = default, Action<SqlOptions> configure = null, [CallerMemberName]string commandName = null)
        {
            IProcResult result = this.ExecuteAndGetResult(commandName, model, new ProcArgs()
            {
                ModelType = typeof(TModel) == typeof(object) ? model?.GetType() : typeof(TModel),
                ResultType = typeof(object),
            });

            SqlOptions options = this.GetCommandOptions(result.Domain);
            configure?.Invoke(options);

            ISqlSerializer<CommandData> serializer = result.Buffer as ISqlSerializer<CommandData>;
            IEnumerable<CommandData> commands = serializer.Serialize(options);

            CommandOptions commandOptions = new CommandOptions()
            {
                ConnectionFactory = result.Domain.ConnectionFactory,
                Filters = options.Filters,
            };

            CommandHandler handler = new CommandHandler(commandOptions);

            handler.Execute(commands);
        }

        /// <summary>
        /// Executes an asynchronous Razor SQL command with the specified model and updates the model from its resulting data set.
        /// </summary>
        /// <param name="model">A concrete model containing parameter values for the command.</param>
        /// <param name="configure">A method for configuring command options.</param>
        /// <param name="commandName">The command name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        protected Task ExecuteAsync(object model = default, Action<SqlOptions> configure = null, [CallerMemberName]string commandName = null, CancellationToken cancellationToken = default)
            => this.ExecuteAsync<object>(model, configure, commandName, cancellationToken);

        /// <summary>
        /// Executes an asynchronous Razor SQL command with the specified model and updates the model from its resulting data set.
        /// </summary>
        /// <typeparam name="TModel">Type of the concrete parameter model.</typeparam>
        /// <param name="model">A concrete model containing parameter values for the command.</param>
        /// <param name="configure">A method for configuring command options.</param>
        /// <param name="commandName">The command name to locate the Razor page by. Defaults to the name of the calling method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        protected async Task ExecuteAsync<TModel>(TModel model = default, Action<SqlOptions> configure = null, [CallerMemberName]string commandName = null, CancellationToken cancellationToken = default)
        {
            IProcResult result = this.ExecuteAndGetResult(commandName, model, new ProcArgs()
            {
                ModelType = typeof(TModel) == typeof(object) ? model?.GetType() : typeof(TModel),
                ResultType = typeof(object),
            });

            SqlOptions options = this.GetCommandOptions(result.Domain);
            configure?.Invoke(options);

            ISqlSerializer<CommandData> serializer = result.Buffer as ISqlSerializer<CommandData>;
            IEnumerable<CommandData> commands = serializer.Serialize(options);

            CommandOptions commandOptions = new CommandOptions()
            {
                ConnectionFactory = result.Domain.ConnectionFactory,
                Filters = options.Filters,
            };

            CommandHandler handler = new CommandHandler(commandOptions);

            await handler.ExecuteAsync(commands, cancellationToken).ConfigureAwait(false);
        }

        private IProcResult ExecuteAndGetResult(string procName, object model, ProcArgs args)
        {
            IProcLocator locator = this.Context?.Locator ?? new ProcLocator();
            IProcEngine engine = this.Context?.Engine ?? new ProcEngine(null);

            PageDescriptor descriptor = locator.FindPage(procName, this.GetType());
            ProcFactory factory = engine.Proc(descriptor, args);

            return factory(model);
        }

        protected virtual SqlOptions GetCommandOptions(IDomainOptions domain) => this.GetDefaultOptions(domain);
        protected virtual SqlOptions GetQueryOptions(IDomainOptions domain) => this.GetDefaultOptions(domain);

        private SqlOptions GetDefaultOptions(IDomainOptions domain)
        {
            return new SqlOptions()
            {
                Filters = domain.Sql.Filters.ToList(),
                MaxParameters = domain.Sql.MaxParameters,
                MaxSql = domain.Sql.MaxSql,
            };
        }
    }
}
