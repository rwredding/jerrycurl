using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.Language
{
    public static class RelationExtensions
    {
        public static RelationHeader Select(this ISchema schema, IEnumerable<string> header)
        {
            IEnumerable<IRelationMetadata> metadata = header.Select(a => schema.GetMetadata<IRelationMetadata>(a)).ToList();
            IReadOnlyList<RelationAttribute> attributes = metadata.Select(m => new RelationAttribute(m)).ToList();

            return new RelationHeader(schema, attributes);
        }

        public static RelationHeader Select(this ISchema schema, params string[] header)
            => schema.Select((IEnumerable<string>)header);

        public static IField2 From<TModel>(this ISchemaStore store, TModel model)
            => new Model2(store.GetSchema(typeof(TModel)), model);

        public static IRelation3 Select(this IField2 source, IEnumerable<string> header)
            => new Relation3(source, source.Identity.Schema.Select(header));

        public static IRelation3 Select(this IField2 source, params string[] header)
            => new Relation3(source, source.Identity.Schema.Select(header));

        public static IRelation3 From(this RelationHeader header, object model)
            => header.From(new Model2(header.Schema, model));

        public static IRelation3 From(this RelationHeader header, IField2 source)
            => new Relation3(source, header);

        public static RelationHeader<TModel> For<TModel>(this ISchemaStore store)
            => new RelationHeader<TModel>(store.GetSchema(typeof(TModel)));
    }
}
