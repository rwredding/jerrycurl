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

        public void Execute(Command command) => this.Execute(new[] { command });
        public void Execute(IEnumerable<Command> commands)
        {
            CommandBuffer buffer = new CommandBuffer();

            using ISyncSession session = this.Options.GetSyncSession();

            foreach (IBatch batch in this.GetBufferedCommands(commands, buffer))
            {
                foreach (IDataReader reader in session.Execute(batch))
                {
                    if (reader.Read())
                        buffer.Update(reader);
                }
            }

            buffer.Commit();
        }

        public Task ExecuteAsync(Command command, CancellationToken cancellationToken = default) => this.ExecuteAsync(new[] { command }, cancellationToken);
        public async Task ExecuteAsync(IEnumerable<Command> commands, CancellationToken cancellationToken = default)
        {
            await using IAsyncSession session = this.Options.GetAsyncSession();

            CommandBuffer buffer = new CommandBuffer();

            foreach (IBatch batch in this.GetBufferedCommands(commands, buffer))
            {
                await foreach (DbDataReader dataReader in session.ExecuteAsync(batch, cancellationToken).ConfigureAwait(false))
                {
                    if (await dataReader.ReadAsync().ConfigureAwait(false))
                        buffer.Update(dataReader);
                }
            }

            buffer.Commit();
        }

        private IEnumerable<IBatch> GetBufferedCommands(IEnumerable<Command> commands, CommandBuffer buffer)
        {
            IEnumerable<Command> filteredCommands = commands.NotNull().Where(d => !string.IsNullOrWhiteSpace(d.CommandText));

            return filteredCommands.Select(c => new BufferedCommand(buffer, c));
        }

        private class BufferedCommand : IBatch
        {
            public CommandBuffer Buffer { get; }
            public Command InnerCommand { get; }

            public BufferedCommand(CommandBuffer buffer, Command innerCommand)
            {
                this.Buffer = buffer;
                this.InnerCommand = innerCommand;
            }

            public void Build(IDbCommand adoCommand)
            {
                foreach (IParameter parameter in this.InnerCommand.Parameters ?? Array.Empty<IParameter>())
                    this.Buffer.Add(parameter);

                foreach (IUpdateBinding binding in this.InnerCommand.Bindings ?? Array.Empty<IUpdateBinding>())
                    this.Buffer.Add(binding);

                adoCommand.CommandText = this.InnerCommand.CommandText;

                foreach (IDbDataParameter parameter in this.Buffer.GetParameters(adoCommand))
                    adoCommand.Parameters.Add(parameter);
            }
        }
    }
}
