using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Data.Metadata.Annotations;
using System.Collections.ObjectModel;
using Jerrycurl.Collections;

namespace Jerrycurl.Mvc.Metadata
{
    public class ProjectionMetadataBuilder : Collection<IProjectionContractResolver>, IMetadataBuilder<IProjectionMetadata>
    {
        public IProjectionContractResolver DefaultResolver { get; set; } = new DefaultProjectionContractResolver();

        public IProjectionMetadata GetMetadata(IMetadataBuilderContext context) => this.GetMetadata(context, context.Identity);

        private IProjectionMetadata GetMetadata(IMetadataBuilderContext context, MetadataIdentity identity)
        {
            MetadataIdentity parentIdentity = identity.Pop();
            IProjectionMetadata parent = context.GetMetadata<IProjectionMetadata>(parentIdentity.Name) ?? this.GetMetadata(context, parentIdentity);

            if (parent == null)
                return null;
            else if (parent.Item != null && parent.Item.Identity.Equals(identity))
                return parent.Item;

            return parent.Properties.FirstOrDefault(m => m.Identity.Equals(identity));
        }

        public void Initialize(IMetadataBuilderContext context)
        {
            IRelationMetadata relation = context.Identity.GetMetadata<IRelationMetadata>();

            if (relation == null)
                throw MetadataNotFoundException.FromMetadata<IRelationMetadata>(context.Identity);

            this.CreateAndAddMetadata(context, relation);
        }

        private Lazy<IReadOnlyList<TItem>> CreateLazy<TItem>(Func<IEnumerable<TItem>> factory) => new Lazy<IReadOnlyList<TItem>>(() => factory().ToList());

        private IEnumerable<ProjectionMetadata> CreateProperties(IMetadataBuilderContext context, ProjectionMetadata parent)
        {
            foreach (IRelationMetadata property in parent.Relation.Properties)
                yield return this.CreateAndAddMetadata(context, property);
        }

        private ProjectionMetadata CreateItem(IMetadataBuilderContext context, ProjectionMetadata parent)
        {
            if (parent.Relation.Item != null)
            {
                ProjectionMetadata metadata = this.CreateBaseMetadata(context, parent.Relation.Item);

                metadata.List = parent;

                context.AddMetadata<IProjectionMetadata>(metadata);

                return metadata;
            }

            return null;
        }

        private ProjectionMetadata CreateAndAddMetadata(IMetadataBuilderContext context, IRelationMetadata relation)
        {
            ProjectionMetadata metadata = this.CreateBaseMetadata(context, relation);

            context.AddMetadata<IProjectionMetadata>(metadata);

            return metadata;
        }

        private ProjectionMetadata CreateBaseMetadata(IMetadataBuilderContext context, IRelationMetadata relation)
        {
            ProjectionMetadata metadata = new ProjectionMetadata(relation);

            metadata.Properties = this.CreateLazy(() => this.CreateProperties(context, metadata));
            metadata.Item = this.CreateItem(context, metadata);

            this.ApplyFlags(metadata);

            return metadata;
        }

        private void ApplyFlags(ProjectionMetadata metadata)
        {
            IEnumerable<IProjectionContractResolver> allResolvers = new[] { this.DefaultResolver }.Concat(this);

            foreach (IProjectionContractResolver resolver in allResolvers)
                metadata.Flags = resolver.GetFlags(metadata);
        }
    }
}
