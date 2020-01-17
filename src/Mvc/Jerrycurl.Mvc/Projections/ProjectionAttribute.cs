using System;
using System.Collections.Generic;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations;

namespace Jerrycurl.Mvc.Projections
{
    public class ProjectionAttribute : IProjectionAttribute
    {
        public IProcContext Context { get; }
        public IProjectionIdentity Identity { get; }

        public IProjectionMetadata Metadata { get; }
        public ISqlContent Content { get; }
        public Func<IField> Field { get; }

        public ProjectionAttribute(IProjection projection)
        {
            this.Identity = projection.Identity;
            this.Metadata = projection.Metadata;
            this.Context = projection.Context;
            this.Content = new SqlContent();
            this.Field = this.GetValueFactory(projection.Source);
        }

        protected ProjectionAttribute(IProjectionAttribute attribute, IProjectionMetadata metadata, ISqlContent content, Func<IField> field)
        {
            if (attribute == null)
                throw ProjectionException.ArgumentNull(nameof(attribute), this);

            this.Context = attribute.Context;
            this.Identity = attribute.Identity;
            this.Metadata = metadata ?? throw ProjectionException.ArgumentNull(nameof(attribute), this);
            this.Content = content ?? throw ProjectionException.ArgumentNull(nameof(content), this);
            this.Field = field;
        }

        private Func<IField> GetValueFactory(IField source)
        {
            if (source == null)
                return null;
            else if (source.Identity.Metadata.Equals(this.Metadata.Identity))
                return () => source;

            Relation relation = new Relation(source, this.Metadata.Identity.Name);

            return () => relation.Scalar();
        }

        public void WriteTo(ISqlBuffer buffer) => this.Content.WriteTo(buffer);
        public override string ToString() => this.Metadata.Identity.ToString();

        public IProjectionAttribute Append(IEnumerable<IParameter> parameters) => this.With(content: this.Content.Append(parameters));
        public IProjectionAttribute Append(IEnumerable<ICommandBinding> bindings) => this.With(content: this.Content.Append(bindings));
        public IProjectionAttribute Append(string text) => this.With(content: this.Content.Append(text));
        public IProjectionAttribute Append(params IParameter[] parameter) => this.With(content: this.Content.Append(parameter));
        public IProjectionAttribute Append(params ICommandBinding[] bindings) => this.With(content: this.Content.Append(bindings));

        public IProjectionAttribute With(IProjectionMetadata metadata = null, ISqlContent content = null, Func<IField> field = null)
        {
            IProjectionMetadata newMetadata = metadata ?? this.Metadata;
            ISqlContent newContent = content ?? this.Content;
            Func<IField> newField = field ?? (metadata != newMetadata ? this.GetValueFactory(this.Field?.Invoke()) : this.Field);

            return new ProjectionAttribute(this, newMetadata, newContent, newField);
        }
    }
}
