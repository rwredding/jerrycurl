using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11
{
    public class RelationFactory
    {
        public ISchemaStore Store { get; }

        public RelationFactory(ISchemaStore store)
        {
            this.Store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IField2 Create<TModel>(TModel model) => new Model2(this.Store.GetSchema(typeof(TModel)), model);
        public RelationBuilder<TItem> Join<TItem>(IList<TItem> model) => this.From(model).Join(s => s);
        public RelationBuilder<TModel> From<TModel>(TModel model) => new RelationBuilder<TModel>(this.Store, this.Create(model));

        public IRelation3 Create<TModel>(TModel model, params string[] header)
        {
            IField2 source = this.Create(model);
            ISchema schema = source.Identity.Schema;
            IReadOnlyList<RelationAttribute> attributes = header.Select(name => new RelationAttribute(schema, name)).ToList();

            return new Relation3(source, new RelationHeader(schema, attributes));
        }
    }
}
