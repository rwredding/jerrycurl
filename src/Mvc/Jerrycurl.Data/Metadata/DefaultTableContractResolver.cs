using System;
using System.Linq;
using System.Reflection;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    public class DefaultTableContractResolver : ITableContractResolver
    {
        public string GetColumnName(ITableMetadata metadata)
        {
            TableAttribute declared = metadata.Relation.Member?.DeclaringType.GetCustomAttribute<TableAttribute>(false);
            ColumnAttribute column = metadata.Relation.Annotations?.OfType<ColumnAttribute>().FirstOrDefault();

            if (metadata.Relation.HasFlag(RelationMetadataFlags.Item))
                column ??= metadata.Relation.Parent.Annotations?.OfType<ColumnAttribute>().FirstOrDefault();

            if (declared != null || column != null || metadata.Relation.HasFlag(RelationMetadataFlags.Item))
                return column?.Name ?? metadata.Relation.Member?.Name ?? metadata.Identity.Notation.Member(metadata.Identity.Name);

            return null;
        }

        public string[] GetTableName(ITableMetadata metadata)
        {
            TableAttribute table = metadata.Relation.Annotations?.OfType<TableAttribute>().FirstOrDefault();

            if (table != null)
            {
                string[] tableName = table.Parts?.ToArray();

                Type declaredType = this.GetDeclaringTypeOfInheritedAttribute(metadata.Relation.Type, table);

                if (metadata.TableName == null || metadata.TableName.Count == 0)
                    tableName = new[] { declaredType?.Name ?? metadata.Relation.Type.Name };

                return tableName;
            }

            return null;
        }

        private Type GetDeclaringTypeOfInheritedAttribute(Type type, Attribute attribute)
        {
            while (type != null && type.BaseType != null && type.BaseType.GetCustomAttributes().Any(a => a.Equals(attribute)))
                type = type.BaseType;

            return type;
        }
    }
}
