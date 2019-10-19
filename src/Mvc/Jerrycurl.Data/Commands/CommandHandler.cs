using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Data.Commands;
using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Data.Filters;
using Jerrycurl.Data.Commands.Internal;
using System.Data.Common;
using System.Threading;

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

            using (AdoConnection connection = new AdoConnection(this.Options))
            {
                foreach (CommandData commandData in commands.NotNull())
                {
                    AdoHelper helper = new AdoHelper(commandData, fieldMap);

                    if (string.IsNullOrWhiteSpace(commandData.CommandText))
                        continue;

                    foreach (IDataReader reader in connection.Execute(helper))
                    {
                        TableIdentity tableInfo = TableIdentity.FromRecord(reader);
                        FieldData[] fields = helper.GetHeading(tableInfo);
                        MetadataIdentity[] metadata = fields.Select(f => f?.Attribute).ToArray();

                        var fun = FuncCache.GetFieldDataBinder(metadata, tableInfo);

                        if (reader.Read())
                            fun(reader, fields);
                    }
                }
            }

            foreach (FieldData fieldData in fieldMap)
                fieldData.Bind();
        }

        public async Task ExecuteAsync(CommandData command, CancellationToken cancellationToken = default) => await this.ExecuteAsync(new[] { command }, cancellationToken);

#if NETSTANDARD2_0
        public async Task ExecuteAsync(IEnumerable<CommandData> commands, CancellationToken cancellationToken = default)
        {
            FieldMap fieldMap = new FieldMap();

            static async Task consumer(AdoHelper helper, DbDataReader reader)
            {
                TableIdentity tableInfo = TableIdentity.FromRecord(reader);
                FieldData[] fields = helper.GetHeading(tableInfo);
                MetadataIdentity[] attributes = fields.Select(f => f.Attribute).ToArray();

                var fun = FuncCache.GetFieldDataBinder(attributes, tableInfo);

                if (await reader.ReadAsync())
                    fun(reader, fields);
            }

            using (AdoConnection connection = new AdoConnection(this.Options))
            {
                foreach (CommandData commandData in commands.NotNull())
                {
                    AdoHelper helper = new AdoHelper(commandData, fieldMap);

                    if (string.IsNullOrWhiteSpace(commandData.CommandText))
                        continue;

                    await connection.ExecuteAsync(helper, r => consumer(helper, r), cancellationToken).ConfigureAwait(false);
                }
            }

            foreach (FieldData fieldData in fieldMap)
                fieldData.Bind();
        }
#elif NETSTANDARD2_1
        public async Task ExecuteAsync(IEnumerable<CommandData> commands, CancellationToken cancellationToken = default)
        {
            FieldMap fieldMap = new FieldMap();

            using (AdoConnection connection = new AdoConnection(this.Options))
            {
                foreach (CommandData commandData in commands.NotNull())
                {
                    AdoHelper helper = new AdoHelper(commandData, fieldMap);

                    if (string.IsNullOrWhiteSpace(commandData.CommandText))
                        continue;

                    await foreach (DbDataReader dataReader in connection.ExecuteAsync(helper, cancellationToken))
                    {
                        TableIdentity tableInfo = TableIdentity.FromRecord(dataReader);
                        FieldData[] fields = helper.GetHeading(tableInfo);
                        MetadataIdentity[] attributes = fields.Select(f => f.Attribute).ToArray();

                        Action<IDataReader, FieldData[]> binder = FuncCache.GetFieldDataBinder(attributes, tableInfo);

                        if (await dataReader.ReadAsync())
                            binder(dataReader, fields);
                    }
                }
            }

            foreach (FieldData fieldData in fieldMap)
                fieldData.Bind();
        }
#endif

    }
}
