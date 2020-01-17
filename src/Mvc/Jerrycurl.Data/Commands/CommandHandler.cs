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

namespace Jerrycurl.Data.Commands
{
    public class CommandHandler
    {
        public CommandOptions Options { get; }

        public CommandHandler(CommandOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Execute(CommandData command) => this.Execute(new[] { command });

        public void Execute(IEnumerable<CommandData> commands)
        {
            FieldMap fieldMap = new FieldMap();

            using SyncSession session = new SyncSession(this.Options);

            foreach (CommandData commandData in commands.NotNull())
            {
                Command command = new Command(commandData, fieldMap);

                if (string.IsNullOrWhiteSpace(commandData.CommandText))
                    continue;

                foreach (IDataReader reader in session.Execute(command))
                {
                    TableIdentity tableInfo = TableIdentity.FromRecord(reader);
                    FieldData[] fields = command.GetHeading(tableInfo);
                    MetadataIdentity[] metadata = fields.Select(f => f?.Attribute).ToArray();

                    var binder = FuncCache.GetFieldDataBinder(metadata, tableInfo);

                    if (reader.Read())
                        binder(reader, fields);
                }
            }

            foreach (FieldData fieldData in fieldMap)
                fieldData.Bind();
        }

        public Task ExecuteAsync(CommandData command, CancellationToken cancellationToken = default) => this.ExecuteAsync(new[] { command }, cancellationToken);

        public async Task ExecuteAsync(IEnumerable<CommandData> commands, CancellationToken cancellationToken = default)
        {
            FieldMap fieldMap = new FieldMap();

            await using AsyncSession session = new AsyncSession(this.Options);

            foreach (CommandData commandData in commands.NotNull())
            {
                Command command = new Command(commandData, fieldMap);

                if (string.IsNullOrWhiteSpace(commandData.CommandText))
                    continue;

                await foreach (DbDataReader dataReader in session.ExecuteAsync(command, cancellationToken).ConfigureAwait(false))
                {
                    TableIdentity tableInfo = TableIdentity.FromRecord(dataReader);
                    FieldData[] fields = command.GetHeading(tableInfo);
                    MetadataIdentity[] attributes = fields.Select(f => f?.Attribute).ToArray();

                    var binder = FuncCache.GetFieldDataBinder(attributes, tableInfo);

                    if (await dataReader.ReadAsync().ConfigureAwait(false))
                        binder(dataReader, fields);
                }
            }

            foreach (FieldData fieldData in fieldMap)
                fieldData.Bind();
        }

    }
}
