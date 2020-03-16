using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Test.Profiling
{
    public class ProfilingCommand : DbCommand
    {
        public DbCommand InnerCommand { get; }

        public ProfilingCommand(DbCommand innerCommand)
            : base()
        {
            this.InnerCommand = innerCommand ?? throw new ArgumentNullException(nameof(innerCommand));
        }

        public override string CommandText
        {
            get => this.InnerCommand.CommandText;
            set => this.InnerCommand.CommandText = value;
        }

        public override CommandType CommandType
        {
            get => this.InnerCommand.CommandType;
            set => this.InnerCommand.CommandType = value;
        }

        public override int CommandTimeout
        {
            get => this.InnerCommand.CommandTimeout;
            set => this.InnerCommand.CommandTimeout = value;
        }

        protected override DbConnection DbConnection
        {
            get => this.InnerCommand.Connection;
            set => this.InnerCommand.Connection = value;
        }

        protected override DbTransaction DbTransaction
        {
            get => this.InnerCommand.Transaction;
            set => this.InnerCommand.Transaction = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => this.InnerCommand.UpdatedRowSource;
            set => this.InnerCommand.UpdatedRowSource = value;
        }

        public override int ExecuteNonQuery() => this.InnerCommand.ExecuteNonQuery();
        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => this.InnerCommand.ExecuteNonQueryAsync(cancellationToken);
        public override object ExecuteScalar() => this.InnerCommand.ExecuteScalar();
        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken) => this.InnerCommand.ExecuteScalarAsync(cancellationToken);
        public override void Prepare() => this.InnerCommand.Prepare();
        public override void Cancel() => this.InnerCommand.Cancel();

        protected override DbParameter CreateDbParameter() => this.InnerCommand.CreateParameter();
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => new ProfilingReader(this.InnerCommand.ExecuteReader(behavior));
        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
            => new ProfilingReader(await this.InnerCommand.ExecuteReaderAsync(behavior, cancellationToken));

        protected override DbParameterCollection DbParameterCollection => this.InnerCommand.Parameters;

        public override bool DesignTimeVisible
        {
            get => this.InnerCommand.DesignTimeVisible;
            set => this.InnerCommand.DesignTimeVisible = value;
        }
    }
}
