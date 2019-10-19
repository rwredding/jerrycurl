using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Test.Profiling
{
    public class ProfilingConnection : DbConnection
    {
        public DbConnection InnerConnection { get; }

        public ProfilingConnection(DbConnection innerConnection)
        {
            this.InnerConnection = innerConnection ?? throw new ArgumentNullException(nameof(innerConnection));
        }

        public override string ConnectionString
        {
            get => this.InnerConnection.ConnectionString;
            set => this.InnerConnection.ConnectionString = value;
        }

        public override string Database => this.InnerConnection.Database;
        public override string DataSource => this.InnerConnection.DataSource;
        public override string ServerVersion => this.InnerConnection.ServerVersion;
        public override ConnectionState State => this.InnerConnection.State;
        public override void ChangeDatabase(string databaseName) => this.InnerConnection.ChangeDatabase(databaseName);
        public override void Close() => this.InnerConnection.Close();
        public override void Open() => this.InnerConnection.Open();
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => this.InnerConnection.BeginTransaction(isolationLevel);
        protected override DbCommand CreateDbCommand() => new ProfilingCommand(this.InnerConnection.CreateCommand());
        public override Task OpenAsync(CancellationToken cancellationToken) => this.InnerConnection.OpenAsync(cancellationToken);
    }
}
