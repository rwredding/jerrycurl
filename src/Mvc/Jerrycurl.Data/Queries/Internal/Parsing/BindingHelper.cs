using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Binding;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Data.Queries.Internal.Extensions;
using Jerrycurl.Reflection;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Parsing
{
    internal static class BindingHelper
    {
        public static ColumnBinder FindValue(Node node, IEnumerable<ColumnName> valueNames)
        {
            foreach (ColumnName value in valueNames)
            {
                MetadataIdentity metadata = new MetadataIdentity(node.Metadata.Identity.Schema, value.Name);

                if (metadata.Equals(node.Identity))
                {
                    return new ColumnBinder(node)
                    {
                        Column = value.Column,
                        CanBeDbNull = true,
                    };
                }
            }

            return null;
        }

        public static void AddPrimaryKey(NewBinder binder)
        {
            IReferenceKey primaryKey = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>()?.Keys.FirstOrDefault(k => k.IsPrimaryKey);

            binder.PrimaryKey = FindPrimaryKey(binder, primaryKey);
        }

        public static KeyBinder FindChildKey(NewBinder binder, IReference reference) => FindKey(binder, reference, reference.FindChildKey());
        public static KeyBinder FindParentKey(NewBinder binder, IReference reference) => FindKey(binder, reference, reference.FindParentKey());
        public static KeyBinder FindPrimaryKey(NewBinder binder, IReferenceKey primaryKey) => FindKey(binder, null, primaryKey);

        public static ParameterExpression Variable(Type type, NodeBinder binder)
            => Variable(type, binder.Metadata?.Identity);

        public static ParameterExpression Variable(Type type, MetadataIdentity identity)
            => Expression.Variable(type, identity?.Name);

        private static KeyBinder FindKey(NewBinder binder, IReference reference, IReferenceKey referenceKey)
        {
            if (referenceKey == null)
                return null;

            List<ValueBinder> values = new List<ValueBinder>();

            foreach (MetadataIdentity identity in referenceKey.Properties.Select(m => m.Identity))
            {
                ValueBinder value = binder.Properties.FirstOfType<ValueBinder>(m => m.Metadata.Identity.Equals(identity));

                values.Add(value);

                if (value != null)
                    value.CanBeDbNull = !referenceKey.IsPrimaryKey;
            }

            if (values.All(v => v != null))
            {
                KeyBinder key = new KeyBinder()
                {
                    Values = values,
                    Metadata = reference,
                };

                if (reference != null)
                {
                    foreach (var (value, keyType) in values.Zip(GetKeyType(reference)))
                        value.KeyType = keyType;

                    key.KeyType = GetCompositeKeyType(values.Select(v => v.KeyType));
                    key.Variable = Expression.Variable(key.KeyType);
                }


                return key;
            }

            return null;
        }

        private static IList<Type> GetKeyType(IReference reference)
        {
            if (reference == null)
                return null;

            List<Type> keyType = new List<Type>();

            foreach (var (left, right) in reference.Key.Properties.Zip(reference.Other.Key.Properties))
            {
                Type leftType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
                Type rightType = Nullable.GetUnderlyingType(right.Type) ?? right.Type;

                if (leftType != rightType)
                    ThrowInvalidKeyException(reference);

                keyType.Add(leftType);
            }

            return keyType;
        }

        private static void ThrowInvalidKeyException(IReference reference)
        {
            string leftTuple = $"({string.Join(", ", reference.Key.Properties.Select(m => m.Type.GetSanitizedName()))})";
            string rightTuple = $"({string.Join(", ", reference.Other.Key.Properties.Select(m => m.Type.GetSanitizedName()))})";


            throw new InvalidOperationException($"Key types are incompatible. Cannot convert {leftTuple} to {rightTuple}.");
        }

        private static Type GetCompositeKeyType(IEnumerable<Type> keyType)
        {
            Type[] typeArray = keyType.ToArray();

            if (typeArray.Length == 0)
                return null;
            else if (typeArray.Length == 1)
                return typeof(CompositeKey<>).MakeGenericType(typeArray[0]);
            else if (typeArray.Length == 2)
                return typeof(CompositeKey<,>).MakeGenericType(typeArray[0], typeArray[1]);
            else if (typeArray.Length == 3)
                return typeof(CompositeKey<,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2]);
            else if (typeArray.Length == 4)
                return typeof(CompositeKey<,,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2], typeArray[3]);
            else
            {
                Type restType = GetCompositeKeyType(keyType.Skip(4));

                return typeof(CompositeKey<,,,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2], typeArray[3], restType);
            }
        }

    }
}
