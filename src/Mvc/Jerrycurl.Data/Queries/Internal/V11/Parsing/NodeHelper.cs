using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binding;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal static class NodeHelper
    {
        public static DataBinder FindData(Node node, ListIdentity identity)
        {
            foreach (ColumnIdentity column in identity.Columns)
            {
                MetadataIdentity metadata = new MetadataIdentity(node.Metadata.Identity.Schema, column.Name);

                if (metadata.Equals(node.Identity))
                {
                    return new DataBinder()
                    {
                        Metadata = node.Metadata,
                        Column = column,
                        CanBeDbNull = true,
                    };
                }
            }

            return null;
        }

        public static KeyBinder FindKey(NewBinder binder, IReferenceKey referenceKey)
        {
            IReadOnlyList<MetadataIdentity> identity = referenceKey?.Properties.Select(m => m.Identity).ToList();

            return FindKey(binder, identity);
        }

        public static KeyBinder FindKey(NewBinder binder, IReadOnlyList<MetadataIdentity> metadata)
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
                KeyBinder key = new KeyBinder()
                {
                    Values = values,
                };

                return key;
            }


            return null;
        }

    }
}
