using Jerrycurl.Reflection;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Relations.Test.Metadata
{
    public class CustomListContractResolver : IRelationContractResolver
    {
        public IEnumerable<Attribute> GetAnnotationContract(IRelationMetadata metadata)
        {
            if (metadata.Parent != null && metadata.Parent.Type.IsOpenGeneric(typeof(CustomList<>), out Type _))
                yield return new CustomAttribute();
        }

        public IRelationListContract GetListContract(IRelationMetadata metadata)
        {
            if (metadata.Type.IsOpenGeneric(typeof(CustomList<>), out Type itemType))
            {
                PropertyInfo indexer = metadata.Type.GetProperties().FirstOrDefault(pi => pi.Name == "Item" && pi.GetIndexParameters().FirstOrDefault()?.ParameterType == typeof(int));

                return new RelationListContract()
                {
                    ItemType = itemType,
                    ReadIndex = indexer.GetMethod,
                    WriteIndex = indexer.SetMethod,
                };
            }

            return null;
        }
    }
}
