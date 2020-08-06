using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Extensions
{
    internal static class MetadataExtensions
    {
        public static IReference Find(this IReference reference, ReferenceFlags flag)
        {
            if (reference.HasFlag(flag))
                return reference;
            else if (reference.Other.HasFlag(flag))
                return reference.Other;
            else
                return null;
        }

        public static IReferenceKey FindKey(this IReference reference, ReferenceFlags flag) => reference.Find(flag)?.Key;
        public static IReferenceKey FindParentKey(this IReference reference) => reference.FindKey(ReferenceFlags.Parent);
        public static IReferenceKey FindChildKey(this IReference reference) => reference.FindKey(ReferenceFlags.Child);

        public static TMetadata To<TMetadata>(this IMetadata metadata)
            where TMetadata : IMetadata
            => metadata.Identity.GetMetadata<TMetadata>();
    }
}
