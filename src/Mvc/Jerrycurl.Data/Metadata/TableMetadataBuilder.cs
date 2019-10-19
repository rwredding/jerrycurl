using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    public class TableMetadataBuilder : IMetadataBuilder<ITableMetadata>
    {
        public ITableMetadata GetMetadata(IMetadataBuilderContext context) => this.GetMetadata(context, context.Identity);

        private ITableMetadata GetMetadata(IMetadataBuilderContext context, MetadataIdentity identity)
        {
            MetadataIdentity parentIdentity = identity.Parent();
            ITableMetadata parent = context.GetMetadata<ITableMetadata>(parentIdentity.Name) ?? this.GetMetadata(context, parentIdentity);

            if (parent == null)
                return null;
            else if (parent.Item != null && parent.Item.Identity.Equals(identity))
                return parent.Item;

            return parent.Properties.FirstOrDefault(m => m.Identity.Equals(identity));
        }

        public void Initialize(IMetadataBuilderContext context)
        {
            IRelationMetadata relation = context.Schema.GetMetadata<IRelationMetadata>(context.Identity.Name);

            if (relation == null)
                throw MetadataNotFoundException.FromMetadata<IRelationMetadata>(context.Identity);

            this.CreateBaseMetadata(context, relation, null);
        }

        private IEnumerable<TableMetadata> CreateProperties(IMetadataBuilderContext context, TableMetadata parent)
        {
            foreach (IRelationMetadata property in parent.Relation.Properties)
                yield return this.CreateBaseMetadata(context, property, parent);
        }

        private TableMetadata CreateItem(IMetadataBuilderContext context, TableMetadata parent)
        {
            if (parent.Relation.Item == null)
                return null;

            return this.CreateBaseMetadata(context, parent.Relation.Item, parent);
        }

        private TableMetadata CreateBaseMetadata(IMetadataBuilderContext context, IRelationMetadata attribute, TableMetadata parent)
        {
            TableMetadata metadata = new TableMetadata(attribute);

            metadata.Item = this.CreateItem(context, metadata);
            metadata.Properties = new Lazy<IReadOnlyList<TableMetadata>>(() => this.CreateProperties(context, metadata).ToList());

            this.AddTableMetadata(metadata);
            this.AddColumnMetadata(metadata, parent);

            context.AddMetadata<ITableMetadata>(metadata);

            return metadata;
        }

        private void AddTableMetadata(TableMetadata metadata)
        {
            TableAttribute table = metadata.Relation.Annotations?.OfType<TableAttribute>().FirstOrDefault();

            if (table != null)
            {
                metadata.Flags |= TableMetadataFlags.Table;
                metadata.TableName = table.Parts?.ToList();

                Type declaredType = this.GetDeclaringTypeOfInheritedAttribute(metadata.Relation.Type, table);

                if (metadata.TableName == null || metadata.TableName.Count == 0)
                    metadata.TableName = new[] { declaredType?.Name ?? metadata.Relation.Type.Name };
            }
        }

        private void AddColumnMetadata(TableMetadata metadata, TableMetadata parent)
        {
            if (parent == null || !parent.HasFlag(TableMetadataFlags.Table) || metadata.HasFlag(TableMetadataFlags.Table))
                return;

            TableAttribute declared = metadata.Relation.Member?.DeclaringType.GetCustomAttribute<TableAttribute>(false);
            ColumnAttribute column = metadata.Relation.Annotations?.OfType<ColumnAttribute>().FirstOrDefault();

            if (metadata.Relation.HasFlag(RelationMetadataFlags.Item))
                column ??= parent.Relation.Annotations?.OfType<ColumnAttribute>().FirstOrDefault();

            if (declared != null || column != null || metadata.Relation.HasFlag(RelationMetadataFlags.Item))
            {
                metadata.ColumnName = column?.Name ?? metadata.Relation.Member?.Name ?? metadata.Identity.Notation.Member(metadata.Identity.Name);
                metadata.MemberOf = parent;

                metadata.Flags |= TableMetadataFlags.Column;
            }
        }

        private Type GetDeclaringTypeOfInheritedAttribute(Type type, Attribute attribute)
        {
            while (type != null && type.BaseType != null && type.BaseType.GetCustomAttributes().Any(a => a.Equals(attribute)))
                type = type.BaseType;

            return type;
        }
    }
}
