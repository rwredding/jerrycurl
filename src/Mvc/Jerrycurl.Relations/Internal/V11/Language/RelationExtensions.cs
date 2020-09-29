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

        public static IEnumerable<ITuple> From(this RelationHeader header, object model)
            => header.Build(model).Body;

        public static IRelation3 Build(this RelationHeader header, object model)
            => new Relation3(new Model2(header.Schema, model), header);

        public static RelationHeader<TModel> For<TModel>(this ISchemaStore store)
            => new RelationHeader<TModel>(store.GetSchema(typeof(TModel)));
    }
}
