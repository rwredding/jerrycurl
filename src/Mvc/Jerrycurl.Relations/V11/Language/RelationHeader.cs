using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.V11.Language
{
    public sealed class RelationHeader<TSource> : RelationHeader
    {
        public RelationAttribute Source { get; }

        public RelationHeader(ISchema schema)
            : base(schema, Array.Empty<RelationAttribute>())
        {
            this.Source = new RelationAttribute(schema, schema.Notation.Model());
        }

        public RelationHeader(ISchema schema, IReadOnlyList<RelationAttribute> attributes)
            : base(schema, attributes)
        {
            this.Source = new RelationAttribute(schema, schema.Notation.Model());
        }

        public RelationHeader(RelationAttribute source, IReadOnlyList<RelationAttribute> attributes)
            : base(source?.Schema, attributes)
        {
            this.Source = source;
        }

        public RelationHeader<TSource> Select()
            => this.Select(m => m);

        public RelationHeader<TSource> Select<TTarget>(Expression<Func<TSource, TTarget>> expression)
        {
            MetadataIdentity newIdentity = this.Source.Metadata.Identity.Push(this.Schema.Notation.Lambda(expression));
            IRelationMetadata metadata = newIdentity.GetMetadata<IRelationMetadata>();

            return new RelationHeader<TSource>(this.Source, this.Add(metadata));
        }

        public RelationHeader<TSource> SelectAll()
            => this.SelectAll(m => m);

        public RelationHeader<TSource> SelectAll<TTarget>(Expression<Func<TSource, TTarget>> expression)
            => this.Select(expression, m => true);

        public RelationHeader<TSource> Select<TTarget>(Expression<Func<TSource, TTarget>> expression, Func<IRelationMetadata, bool> selector)
        {
            MetadataIdentity sourceIdentity = this.Source.Metadata.Identity.Push(this.Schema.Notation.Lambda(expression));
            IReadOnlyList<IRelationMetadata> metadata = sourceIdentity.GetMetadata<IRelationMetadata>().Properties;

            return new RelationHeader<TSource>(this.Source, this.Add(metadata.Where(selector)));
        }

        public RelationHeader<TTarget> Join<TTarget>(Expression<Func<TSource, IEnumerable<TTarget>>> expression)
        {
            MetadataIdentity newIdentity = this.Source.Metadata.Identity.Push(this.Schema.Notation.Lambda(expression));
            IRelationMetadata metadata = newIdentity.GetMetadata<IRelationMetadata>();
            RelationAttribute newSource = new RelationAttribute(metadata.Item);

            return new RelationHeader<TTarget>(newSource, Array.Empty<RelationAttribute>());
        }

        private IReadOnlyList<RelationAttribute> Add(IRelationMetadata metadata)
            => this.Attributes.Append(new RelationAttribute(metadata)).ToList();

        private IReadOnlyList<RelationAttribute> Add(IEnumerable<IRelationMetadata> metadata)
            => this.Attributes.Concat(metadata.Select(m => new RelationAttribute(m))).ToList();
    }
}
