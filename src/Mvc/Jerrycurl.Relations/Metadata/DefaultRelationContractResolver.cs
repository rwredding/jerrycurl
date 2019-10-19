using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace Jerrycurl.Relations.Metadata.Contracts
{
    public class DefaultRelationContractResolver : IRelationContractResolver
    {
        public IRelationListContract GetListContract(IRelationMetadata metadata)
        {
            if (this.IsEnumerable(metadata))
            {
                return new RelationListContract()
                {
                    ItemType = this.GetGenericItemType(metadata),
                    ReadIndex = this.GetListIndexReader(metadata),
                    WriteIndex = this.GetListIndexWriter(metadata),
                };
            }
            else if (this.IsOneDimensionalArray(metadata))
            {
                return new RelationListContract()
                {
                    ItemType = this.GetArrayItemType(metadata),
                    ReadIndex = this.GetArrayIndexReader(metadata),
                    WriteIndex = this.GetArrayIndexWriter(metadata),
                };
            }

            return null;
        }

        public IEnumerable<Attribute> GetAnnotationContract(IRelationMetadata metadata)
        {
            return metadata.Type.GetCustomAttributes(true).OfType<Attribute>().Concat(metadata.Member?.GetCustomAttributes() ?? Array.Empty<Attribute>());
        }

        private bool IsEnumerable(IRelationMetadata metadata)
        {
            if (!metadata.Type.IsGenericType)
                return false;

            Type[] allowedTypes = new Type[]
            {
                typeof(IList<>),
                typeof(List<>),
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(IReadOnlyList<>),
                typeof(IReadOnlyCollection<>),
                typeof(Many<>),
            };

            Type openType = metadata.Type.GetGenericTypeDefinition();

            if (!allowedTypes.Contains(openType))
                return false;

            return true;
        }


        private bool IsOneDimensionalArray(IRelationMetadata metadata) => (metadata.Type.IsArray && metadata.Type.GetArrayRank() == 1);

        private Type GetGenericItemType(IRelationMetadata metadata) => metadata.Type.GetGenericArguments()[0];

        private MethodInfo GetListIndexWriter(IRelationMetadata metadata) => this.GetListIndexer(metadata)?.SetMethod;
        private MethodInfo GetListIndexReader(IRelationMetadata metadata) => this.GetListIndexer(metadata)?.GetMethod;

        private Type GetArrayItemType(IRelationMetadata metadata) => metadata.Type.GetElementType();
        private MethodInfo GetArrayIndexWriter(IRelationMetadata metadata) => metadata.Type.GetMethod("Set", new[] { typeof(int), metadata.Type });
        private MethodInfo GetArrayIndexReader(IRelationMetadata metadata) => metadata.Type.GetMethod("Get", new[] { typeof(int) });

        private PropertyInfo GetListIndexer(IRelationMetadata metadata)
        {
            Type[] allowedTypes = new Type[]
            {
                typeof(IList<>),
                typeof(List<>),
                typeof(IReadOnlyList<>),
            };

            Type openType = metadata.Type.GetGenericTypeDefinition();

            if (!allowedTypes.Contains(openType))
                return null;

            return metadata.Type.GetProperties().FirstOrDefault(pi => pi.Name == "Item" && pi.GetIndexParameters().FirstOrDefault()?.ParameterType == typeof(int));
        }
    }
}
