using Jerrycurl.Reflection;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jerrycurl.Relations.Test.Metadata
{
    public class InvalidContractResolver : IRelationContractResolver
    {
        public IEnumerable<Attribute> GetAnnotationContract(IRelationMetadata metadata) => Array.Empty<Attribute>();

        public IRelationListContract GetListContract(IRelationMetadata metadata)
        {
            if (metadata.Member == typeof(CustomModel).GetProperty(nameof(CustomModel.List1)))
            {
                return new RelationListContract()
                {
                    ItemType = typeof(string),
                };
            }
            else if (metadata.Member == typeof(CustomModel).GetProperty(nameof(CustomModel.List2)))
            {
                return new RelationListContract()
                {
                    ItemType = typeof(int),
                    ReadIndex = typeof(List<int>).GetMethod("AddRange"),
                };
            }
            else if (metadata.Member == typeof(CustomModel).GetProperty(nameof(CustomModel.List3)))
            {
                PropertyInfo indexer = this.GetIndexer(metadata.Type, typeof(int));

                return new RelationListContract()
                {
                    ItemType = typeof(int),
                    ReadIndex = indexer.GetMethod,
                    WriteIndex = typeof(List<int>).GetMethod("AddRange"),
                };
            }
            else if (metadata.Type.IsOpenGeneric(typeof(CustomList<>), out Type itemType))
            {
                PropertyInfo indexer = this.GetIndexer(metadata.Type, typeof(int));

                return new RelationListContract()
                {
                    ItemType = itemType,
                    //Indexer = indexer,
                    ReadIndex = indexer.GetMethod,
                    WriteIndex = indexer.SetMethod,
                };
            }

            return null;
        }

        private PropertyInfo GetIndexer(Type type, Type parameterType)
        {
            return type.GetProperties().FirstOrDefault(pi => pi.Name == "Item" && pi.GetIndexParameters().FirstOrDefault()?.ParameterType == parameterType);
        }
    }
}
