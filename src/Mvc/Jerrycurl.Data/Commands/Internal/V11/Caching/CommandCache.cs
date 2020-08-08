using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Commands.Internal.Caching;
using Jerrycurl.Data.Commands.Internal.V11.Compilation;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Commands.Internal.V11.Caching
{
    internal static class CommandCache
    {
        private static readonly ConcurrentDictionary<CommandCacheKey, BufferWriter> writerMap = new ConcurrentDictionary<CommandCacheKey, BufferWriter>();

        public static BufferWriter GetWriter(ISchema schema, IReadOnlyList<ColumnName> columnNames)
        {
            CommandCacheKey cacheKey = new CommandCacheKey(schema, columnNames);

            return writerMap.GetOrAdd(cacheKey, k =>
            {
                CommandCompiler compiler = new CommandCompiler();

                return compiler.Compile(k.Items);
            });
        }
    }
}
