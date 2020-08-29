using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11
{
    public sealed class RelationBuilder<TSource>
    {
        public ISchema Schema { get; }
        public IRelationMetadata Metadata { get; }
        public IField2 Source { get; }
        public IEnumerable<RelationAttribute> Attributes => this.attributes;

        private List<RelationAttribute> attributes;

        public RelationBuilder(ISchemaStore store, IField2 source = null)
        {
            this.Schema = store.GetSchema(typeof(TSource));
            this.Metadata = this.Schema.GetMetadata<IRelationMetadata>();
            this.Source = source;
            this.attributes = new List<RelationAttribute>();
        }

        private RelationBuilder(IRelationMetadata metadata, IField2 source, List<RelationAttribute> attributes)
        {
            this.Metadata = metadata;
            this.Schema = metadata.Identity.Schema;
            this.Source = source;
            this.attributes = attributes;
        }

        public RelationBuilder<TTarget> From<TTarget>(Expression<Func<TSource, IEnumerable<TTarget>>> expression)
        {
            MetadataIdentity newIdentity = this.Metadata.Identity.Push(this.Schema.Notation.Lambda(expression));
            IRelationMetadata metadata = newIdentity.GetMetadata<IRelationMetadata>();

            return new RelationBuilder<TTarget>(metadata, this.Source, new List<RelationAttribute>());
        }

        public RelationBuilder<TSource> Select<TTarget>(Expression<Func<TSource, TTarget>> expression)
        {
            MetadataIdentity newIdentity = this.Metadata.Identity.Push(this.Schema.Notation.Lambda(expression));
            IRelationMetadata metadata = newIdentity.GetMetadata<IRelationMetadata>();

            this.attributes.Add(new RelationAttribute(metadata));

            return this;
        }

        public RelationBuilder<TTarget> Join<TTarget>(Expression<Func<TSource, IEnumerable<TTarget>>> expression)
        {
            MetadataIdentity newIdentity = this.Metadata.Identity.Push(this.Schema.Notation.Lambda(expression));
            IRelationMetadata metadata = newIdentity.GetMetadata<IRelationMetadata>();

            return new RelationBuilder<TTarget>(metadata.Item, this.Source, this.attributes);
        }

        public RelationHeader ToHeader() => new RelationHeader(this.Schema, this.attributes.ToList());
        public Relation3 ToRelation(IField2 source = null) => new Relation3(this.GetRequiredSource(source), this.ToHeader());
        public IRelationReader ToReader(IField2 source = null) => this.ToRelation(this.GetRequiredSource(source)).GetReader();

        private IField2 GetRequiredSource(IField2 source)
            => source ?? this.Source ?? throw new InvalidOperationException("No source specified.");
    }
}
