using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Vendors.SqlServer.Internal
{
    internal static class TvpCache
    {
        public static ConcurrentDictionary<RelationIdentity, IBindingParameterContract> Resolvers { get; } = new ConcurrentDictionary<RelationIdentity, IBindingParameterContract>();

        public static IBindingParameterContract GetParameterContract(RelationIdentity relation)
        {
            if (relation == null)
                throw new ArgumentNullException(nameof(relation));

            return Resolvers.GetOrAdd(relation, _ =>
            {
                IBindingMetadata[] bindings = relation.Heading.Select(m => m.GetMetadata<IBindingMetadata>()).ToArray();

                return TvpHelper.GetParameterContract(bindings);
            });
        }
    }
}
