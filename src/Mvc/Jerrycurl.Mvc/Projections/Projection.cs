using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Collections;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Projections
{
    public class Projection : IProjection
    {
        public IProjectionMetadata Metadata { get; }
        public IProjectionIdentity Identity { get; }
        public IProcContext Context { get; }
        public IEnumerable<IProjectionAttribute> Attributes { get; }
        public IProjectionOptions Options { get; }
        public IField Source { get; }

        public Projection(IProjectionIdentity identity, IProcContext context)
        {
            this.Identity = identity ?? throw ProjectionException.ArgumentNull(nameof(identity));
            this.Context = context ?? throw ProjectionException.ArgumentNull(nameof(context));
            this.Metadata = identity.Schema?.GetMetadata<IProjectionMetadata>() ?? throw ProjectionException.FromAttribute(identity.Schema.Model, null, message: "Projection metadata not found.");
            this.Source = identity.Field;
            this.Options = new ProjectionOptions();
            this.Attributes = this.CreateDefaultAttributes(this.Metadata);
        }

        internal Projection(IProjectionIdentity identity, IProcContext context, IProjectionMetadata metadata)
        {
            this.Identity = identity ?? throw ProjectionException.ArgumentNull(nameof(identity));
            this.Context = context ?? throw ProjectionException.ArgumentNull(nameof(context));
            this.Metadata = metadata ?? throw ProjectionException.FromAttribute(identity.Schema.Model, null, message: "Projection metadata not found.");
            this.Source = identity.Field;
            this.Options = new ProjectionOptions();
            this.Attributes = this.CreateDefaultAttributes(this.Metadata);
        }

        protected Projection(IProjection projection)
        {
            if (projection == null)
                throw ProjectionException.ArgumentNull(nameof(projection));

            this.Identity = projection.Identity;
            this.Context = projection.Context;
            this.Metadata = projection.Metadata;
            this.Attributes = projection.Attributes;
            this.Options = projection.Options;
            this.Source = projection.Source;
        }

        protected Projection(IProjection projection, IProjectionMetadata metadata, IEnumerable<IProjectionAttribute> attributes, IField field, IProjectionOptions options)
        {
            if (projection == null)
                throw ProjectionException.ArgumentNull(nameof(projection));

            this.Identity = projection.Identity;
            this.Context = projection.Context;
            this.Metadata = metadata ?? throw ProjectionException.ArgumentNull(nameof(projection));
            this.Attributes = attributes ?? throw ProjectionException.ArgumentNull(nameof(attributes));
            this.Options = options ?? throw ProjectionException.ArgumentNull(nameof(options));
            this.Source = field;
        }

        private IEnumerable<IProjectionMetadata> SelectAttributes(IProjectionMetadata metadata)
        {
            if (metadata.HasFlag(RelationMetadataFlags.List) && metadata.Item.HasFlag(TableMetadataFlags.Column))
                return new[] { metadata.Item };
            if (metadata.HasFlag(RelationMetadataFlags.List) && metadata.Item.HasFlag(TableMetadataFlags.Table))
                return metadata.Item.Properties.Where(a => a.HasFlag(TableMetadataFlags.Column));
            else if (metadata.HasFlag(TableMetadataFlags.Table))
                return metadata.Properties.Where(a => a.HasFlag(TableMetadataFlags.Column));

            return metadata.Properties;
        }


        private IEnumerable<IProjectionAttribute> CreateDefaultAttributes(IProjectionMetadata metadata)
        {
            IEnumerable<IProjectionMetadata> attributes = this.SelectAttributes(metadata);
            IEnumerable<Func<IField>> fields = attributes.Select(_ => (Func<IField>)null);
            IField source = this.Source;

            if (source != null)
            {
                Lazy<ITuple> tuple = new Lazy<ITuple>(() =>
                {
                    Relation relation = new Relation(source, attributes.Select(m => m.Identity));

                    return relation.Row();
                });

                fields = attributes.Select((_, i) => new Func<IField>(() => tuple.Value[i]));
            }

            foreach (var (m, f) in attributes.Zip(fields))
                yield return new ProjectionAttribute(this).With(metadata: m, field: f);
        }

        public IProjection Map(Func<IProjectionAttribute, IProjectionAttribute> m) => this.With(attributes: this.Attributes.Select(a => m(a)));

        public IProjection Append(IEnumerable<IParameter> parameters) => this.Map(a => a.Append(parameters));
        public IProjection Append(IEnumerable<ICommandBinding> bindings) => this.Map(a => a.Append(bindings));
        public IProjection Append(string text) => this.Map(a => a.Append(text));
        public IProjection Append(params IParameter[] parameter) => this.Map(a => a.Append(parameter));
        public IProjection Append(params ICommandBinding[] bindings) => this.Map(a => a.Append(bindings));

        public void WriteTo(ISqlBuffer buffer)
        {
            IProjectionAttribute[] attributes = this.Attributes.ToArray();

            if (attributes.Length > 0)
            {
                attributes[0].WriteTo(buffer);

                foreach (IProjectionAttribute attr in attributes.Skip(1))
                {
                    buffer.Append(this.Options.Separator);

                    attr.WriteTo(buffer);
                }
            }
        }

        public IProjection With(IProjectionMetadata metadata = null,
                                IEnumerable<IProjectionAttribute> attributes = null,
                                IField field = null,
                                IProjectionOptions options = null)
        {
            IProjectionMetadata newMetadata = metadata ?? this.Metadata;
            IEnumerable<IProjectionAttribute> newAttributes = attributes ?? (newMetadata != this.Metadata ? this.CreateDefaultAttributes(newMetadata) : this.Attributes);
            IField newField = field ?? this.Source;
            IProjectionOptions newOptions = options ?? this.Options;

            return new Projection(this, newMetadata, newAttributes, newField, newOptions);
        }
    }
}
