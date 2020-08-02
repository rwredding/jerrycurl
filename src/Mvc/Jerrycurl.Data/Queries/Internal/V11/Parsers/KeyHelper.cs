using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal static class KeyHelper
    {
        public static ValueKey FindKey(NewBinder binder, IReadOnlyList<IReferenceMetadata> metadata)
        {
            IReadOnlyList<MetadataIdentity> identity = metadata.Select(m => m.Identity).ToList();

            return FindKey(binder, metadata);
        }

        public static ValueKey FindKey(NewBinder binder, IReadOnlyList<MetadataIdentity> metadata)
        {
            if (metadata == null)
                return null;

            List<ValueBinder> values = new List<ValueBinder>();

            foreach (MetadataIdentity identity in metadata)
            {
                ValueBinder databinder = binder.Properties.OfType<ValueBinder>().FirstOrDefault(r => r.Metadata.Identity.Equals(identity));

                values.Add(databinder);
            }

            if (values.Count == metadata.Count)
            {
                ValueKey key = new ValueKey()
                {
                    Values = values,
                };

                return key;
            }


            return null;
        }

    }
}
