using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Mvc.Metadata
{
    public class ProjectionMetadataBuilder : IMetadataBuilder<IProjectionMetadata>
    {
        public IProjectionMetadata GetMetadata(IMetadataBuilderContext context) => this.GetMetadata(context, context.Identity);

        private IProjectionMetadata GetMetadata(IMetadataBuilderContext context, MetadataIdentity identity)
        {
            MetadataIdentity parentIdentity = identity.Parent();
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
            metadata.Flags = this.GetFlags(metadata);

            return metadata;
        }

        private ProjectionMetadataFlags GetFlags(ProjectionMetadata metadata)
        {
            IdAttribute id = metadata.Relation.Annotations?.OfType<IdAttribute>().FirstOrDefault();
            OutAttribute out0 = metadata.Relation.Annotations?.OfType<OutAttribute>().FirstOrDefault();
            InAttribute in0 = metadata.Relation.Annotations?.OfType<InAttribute>().FirstOrDefault();

            IReferenceMetadata reference = metadata.Identity.GetMetadata<IReferenceMetadata>();
            ProjectionMetadataFlags flags = ProjectionMetadataFlags.None;

            if (id != null)
                flags |= ProjectionMetadataFlags.Identity;

            if (in0 != null || out0 != null)
            {
                flags |= in0 != null ? ProjectionMetadataFlags.Input : ProjectionMetadataFlags.None;
                flags |= out0 != null ? ProjectionMetadataFlags.Output : ProjectionMetadataFlags.None;
            }
            else if (id != null)
                flags |= ProjectionMetadataFlags.Output;
            else if (reference != null && reference.HasAnyFlag(ReferenceMetadataFlags.Key))
                flags |= ProjectionMetadataFlags.Input | ProjectionMetadataFlags.Output;
            else
                flags |= ProjectionMetadataFlags.Input;

            return flags;
        }
    }
}
