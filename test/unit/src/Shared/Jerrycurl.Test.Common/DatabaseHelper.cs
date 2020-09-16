using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Filters;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Test.Profiling;
using Jerrycurl.Vendors.Sqlite.Metadata;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jerrycurl.Test
{
    public class DatabaseHelper
    {
        public const string TestDbConnectionString = "DATA SOURCE=testdb.db";

        public static DatabaseHelper Default { get; } = new DatabaseHelper();

        public SchemaStore Schemas { get; set; }
        public QueryOptions QueryOptions { get; set; }
        public CommandOptions CommandOptions { get; set; }

        public DatabaseHelper()
        {
            this.Schemas = this.GetSchemas();
            this.QueryOptions = this.GetQueryOptions();
            this.CommandOptions = this.GetCommandOptions();
        }

        public SchemaStore GetSchemas()
        {
            RelationMetadataBuilder relationBuilder = new RelationMetadataBuilder();
            BindingMetadataBuilder bindingBuilder = new BindingMetadataBuilder();
            ReferenceMetadataBuilder referenceBuilder = new ReferenceMetadataBuilder();

            SchemaStore store = new SchemaStore(new DotNotation(), relationBuilder, bindingBuilder, referenceBuilder);

            bindingBuilder.Add(new SqliteContractResolver());

            return store;
        }

        public QueryOptions GetQueryOptions(SchemaStore schemas = null)
        {
            return new QueryOptions()
            {
                ConnectionFactory = () => new ProfilingConnection(new SqliteConnection(TestDbConnectionString)),
                Schemas = schemas ?? this.Schemas,
            };
        }

        public CommandOptions GetCommandOptions(params IFilter[] filters)
        {
            return new CommandOptions()
            {
                ConnectionFactory = () => new ProfilingConnection(new SqliteConnection(TestDbConnectionString)),
                Filters = filters ?? Array.Empty<IFilter>(),
            };
        }

        public QueryEngine Queries => new QueryEngine(this.QueryOptions);
        public CommandEngine Commands => new CommandEngine(this.CommandOptions);

        public async Task ExecuteAsync(params Command[] commands) => await this.Commands.ExecuteAsync(commands);
        public void Execute(params Command[] commands) => this.Commands.Execute(commands);

        public async Task<IList<TItem>> QueryAsync<TItem>(params SqliteTable[] tables) => await this.Queries.ListAsync<TItem>(tables.Select(t => t.ToQuery()));
        public async Task<IList<TItem>> QueryAsync<TItem>(params Query[] queries) => await this.Queries.ListAsync<TItem>(queries);
        public async Task<IList<TItem>> QueryAsync<TItem>(string sql) => await this.Queries.ListAsync<TItem>(new Query() { QueryText = sql });

        public IList<TItem> Query<TItem>(params SqliteTable[] tables) => this.Queries.List<TItem>(tables.Select(t => t.ToQuery()));
        public IList<TItem> Query<TItem>(params Query[] queries) => this.Queries.List<TItem>(queries);
        public IList<TItem> Query<TItem>(string sql) => this.Queries.List<TItem>(new Query() { QueryText = sql });

        public TItem Aggregate<TItem>(params SqliteTable[] tables) => this.Queries.Aggregate<TItem>(tables.Select(t => t.ToQuery()));
        public TItem Aggregate<TItem>(params Query[] queries) => this.Queries.Aggregate<TItem>(queries);
        public TItem Aggregate<TItem>(string sql) => this.Queries.Aggregate<TItem>(new Query() { QueryText = sql });

        public async Task<TItem> AggregateAsync<TItem>(params SqliteTable[] tables) => await this.Queries.AggregateAsync<TItem>(tables.Select(t => t.ToQuery()));
        public async Task<TItem> AggregateAsync<TItem>(params Query[] queries) => await this.Queries.AggregateAsync<TItem>(queries);
        public async Task<TItem> AggregateAsync<TItem>(string sql) => await this.Queries.AggregateAsync<TItem>(new Query() { QueryText = sql });


        public IEnumerable<TItem> Enumerate<TItem>(params SqliteTable[] tables) => this.Queries.Enumerate<TItem>(tables.Select(t => t.ToQuery()));
        public IEnumerable<TItem> Enumerate<TItem>(params Query[] queries) => this.Queries.Enumerate<TItem>(queries);
        public IAsyncEnumerable<TItem> EnumerateAsync<TItem>(params SqliteTable[] tables) => this.Queries.EnumerateAsync<TItem>(tables.Select(t => t.ToQuery()));
        public IAsyncEnumerable<TItem> EnumerateAsync<TItem>(params Query[] queries) => this.Queries.EnumerateAsync<TItem>(queries);
        public IAsyncEnumerable<TItem> EnumerateAsync<TItem>(string sql) => this.Queries.EnumerateAsync<TItem>(new Query() { QueryText = sql });

        public Relation Relation<T>(T model = default, params string[] heading)
        {
            ISchema schema = this.Schemas.GetSchema(typeof(T));

            return new Relation(model, schema, heading.Select(n => new MetadataIdentity(schema, n)));
        }

        public IField Field<T>(T value = default)
        {
            ISchema schema = this.Schemas.GetSchema(typeof(T));

            Relation relation = new Relation(value, schema);

            return relation;
        }
    }
}
