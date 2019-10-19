using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Nodes;
using Jerrycurl.Data.Queries.Internal.State;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class ResultCache<TItem>
    {
        private static readonly ConcurrentDictionary<TableIdentity, object> factoryMap = new ConcurrentDictionary<TableIdentity, object>();
        private static TypeState typeState;
        private static readonly object typeLock = new object();

        private static TypeState GetTypeState(ISchemaStore schemas)
        {
            if (typeState != null)
                return typeState;

            lock (typeLock)
            {
                if (typeState != null)
                    return typeState;

                Type listType = typeof(IList<>).MakeGenericType(typeof(TItem));

                ISchema schema = schemas.GetSchema(listType);

                return (typeState = new TypeState(schema));
            }
        }

        public static ResultState<TItem> GetResultState(ISchemaStore schemas, TableIdentity heading)
        {
            return (ResultState<TItem>)factoryMap.GetOrAdd(heading, _ =>
            {
                TypeState state = GetTypeState(schemas);

                return new ResultCompiler(state).Compile<TItem>(heading);
            });
        }
    }
}
