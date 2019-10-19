using Jerrycurl.Data.Queries.Internal.State;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class ResultAdapter<TItem>
    {
        public ISchemaStore Schemas { get; }

        public ExpandingArray Lists { get; } = new ExpandingArray();
        public ExpandingArray Dicts { get; } = new ExpandingArray();
        public ExpandingArray<bool> Bits { get; } = new ExpandingArray<bool>();

        private ResultState<TItem> state;

        public ResultAdapter(ISchemaStore schemas)
        {
            this.Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
        }

        private void SetState(IDataReader dataReader)
        {
            TableIdentity heading = TableIdentity.FromRecord(dataReader);

            this.state = ResultCache<TItem>.GetResultState(this.Schemas, heading);
        }

        public void AddResult(IDataReader dataReader)
        {
            this.SetState(dataReader);

            this.state.List(dataReader, this.Lists, this.Dicts, this.Bits);
        }

        public async Task AddResultAsync(DbDataReader dataReader, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            this.SetState(dataReader);

            this.state.Initializer(this.Lists, this.Dicts, this.Bits);

            while (await dataReader.ReadAsync(cancellationToken))
                this.state.ListItem(dataReader, this.Lists, this.Dicts);
        }

        public IList<TItem> ToList()
        {
            if (this.Lists[0] == null)
                this.state?.Aggregate.Factory(this.Lists, this.Bits);

            return (IList<TItem>)this.Lists[0];
        }
    }
}
