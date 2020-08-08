using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.Collections;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Data.Commands.Internal;
using System.Data.Common;
using System.Threading;
using Jerrycurl.Data.Sessions;
using System.IO;

namespace Jerrycurl.Data.Commands
{
    public class CommandEngine
    {
        public CommandOptions Options { get; }

        public CommandEngine(CommandOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Execute(CommandData command) => this.Execute(new[] { command });
        public void Execute(IEnumerable<CommandData> commands)
        {
            CommandBuffer buffer = new CommandBuffer();

            using ISyncSession session = this.Options.GetSyncSession();

            foreach (IOperation operation in this.GetOperations(buffer, commands))
            {
                foreach (IDataReader reader in session.Execute(operation))
                {
                    if (reader.Read())
                        buffer.Write(reader);
                }
            }

            buffer.Commit();
        }

        public Task ExecuteAsync(CommandData command, CancellationToken cancellationToken = default) => this.ExecuteAsync(new[] { command }, cancellationToken);
        public async Task ExecuteAsync(IEnumerable<CommandData> commands, CancellationToken cancellationToken = default)
        {
            CommandBuffer buffer = new CommandBuffer();

            await using IAsyncSession session = this.Options.GetAsyncSession();

            foreach (IOperation operation in this.GetOperations(buffer, commands))
            {
                await foreach (DbDataReader dataReader in session.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false))
                {
                    if (await dataReader.ReadAsync().ConfigureAwait(false))
                        buffer.Write(dataReader);
                }
            }

            buffer.Commit();
        }

        private IEnumerable<IOperation> GetOperations(CommandBuffer buffer, IEnumerable<CommandData> commands)
            => commands.NotNull().Where(d => !string.IsNullOrWhiteSpace(d.CommandText)).Select(buffer.Read);
    }
}
