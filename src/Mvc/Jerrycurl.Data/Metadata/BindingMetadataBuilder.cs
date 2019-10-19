using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using Jerrycurl.Collections;
using Jerrycurl.Reflection;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    public class BindingMetadataBuilder : Collection<IBindingContractResolver>, IBindingMetadataBuilder
    {
        public IBindingContractResolver DefaultResolver { get; set; } = new DefaultBindingContractResolver();

        public IBindingMetadata GetMetadata(IMetadataBuilderContext context) => this.GetMetadata(context, context.Identity);

        private IBindingMetadata GetMetadata(IMetadataBuilderContext context, MetadataIdentity identity)
        {
            MetadataIdentity parentIdentity = identity.Parent();
            IBindingMetadata parent = context.GetMetadata<IBindingMetadata>(parentIdentity.Name) ?? this.GetMetadata(context, parentIdentity);

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

        private Lazy<IReadOnlyList<BindingMetadata>> CreateLazyProperties(IMetadataBuilderContext context, BindingMetadata parent)
        {
            return new Lazy<IReadOnlyList<BindingMetadata>>(() => this.CreateProperties(context, parent).ToList());
        }

        private IEnumerable<BindingMetadata> CreateProperties(IMetadataBuilderContext context, BindingMetadata parent)
        {
            foreach (IRelationMetadata attribute in parent.Relation.Properties)
                yield return this.CreateBaseMetadata(context, attribute, parent);
        }

        private BindingMetadata CreateItem(IMetadataBuilderContext context, BindingMetadata parent)
        {
            if (parent.Relation.Item == null)
                return null;

            return this.CreateBaseMetadata(context, parent.Relation.Item, parent);
        }

        private BindingMetadata CreateBaseMetadata(IMetadataBuilderContext context, IRelationMetadata attribute, BindingMetadata parent)
        {
            BindingMetadata metadata = new BindingMetadata(attribute)
            {
                Parent = parent,
                MemberOf = parent?.MemberOf
            };

            if (metadata.MemberOf == null || metadata.HasFlag(RelationMetadataFlags.Item))
                metadata.MemberOf = metadata;

            metadata.Properties = this.CreateLazyProperties(context, metadata);
            metadata.Item = this.CreateItem(context, metadata);
            metadata.Flags = this.GetFlags(metadata);

            context.AddMetadata<IBindingMetadata>(metadata);

            this.ApplyContracts(metadata);

            return metadata;
        }

        private BindingMetadataFlags GetFlags(BindingMetadata metadata)
        {
            BindingMetadataFlags flags = BindingMetadataFlags.None;

            if (metadata.Parent == null)
                flags |= BindingMetadataFlags.Model;

            if (metadata.Relation.HasFlag(RelationMetadataFlags.List))
                flags |= BindingMetadataFlags.List;

            if (metadata.Relation.HasFlag(RelationMetadataFlags.Item))
                flags |= BindingMetadataFlags.Item;

            if (metadata.Type == typeof(object) || metadata.Type == typeof(ExpandoObject))
                flags |= BindingMetadataFlags.Dynamic;

            if (metadata.Relation.HasFlag(RelationMetadataFlags.Readable))
                flags |= BindingMetadataFlags.Readable;

            if (metadata.Relation.HasFlag(RelationMetadataFlags.Writable))
                flags |= BindingMetadataFlags.Writable;

            return flags;
        }

        private void ApplyContracts(BindingMetadata metadata)
        {
            IEnumerable<IBindingContractResolver> allResolvers = this;

            if (this.DefaultResolver != null)
                allResolvers = new[] { this.DefaultResolver }.Concat(allResolvers);

            foreach (IBindingContractResolver resolver in allResolvers.NotNull().OrderBy(r => r.Priority))
            {
                IBindingParameterContract parameter = resolver.GetParameterContract(metadata);
                IBindingCompositionContract composition = resolver.GetCompositionContract(metadata);
                IBindingValueContract value = resolver.GetValueContract(metadata);
                IBindingHelperContract helper = resolver.GetHelperContract(metadata);

                metadata.Parameter = parameter ?? metadata.Parameter;
                metadata.Composition = composition ?? metadata.Composition;
                metadata.Value = value ?? metadata.Value;
                metadata.Helper = helper ?? metadata.Helper;
            }

            this.ValidateComposition(metadata);
        }

        private void ValidateComposition(BindingMetadata metadata)
        {
            if (metadata.Composition == null || metadata.Composition.Construct == null)
                return;

            Type newType = metadata.Composition.Construct.Type;

            if (!metadata.Type.IsAssignableFrom(newType))
                throw new MetadataBuilderException($"Constructor of type '{newType.GetSanitizedName()}' cannot be assigned to property of type '{metadata.Type}'.");
        }
    }
}
