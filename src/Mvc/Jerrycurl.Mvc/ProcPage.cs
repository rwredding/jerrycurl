using System;
using System.Linq;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc
{
    public class ProcPage<TModel, TResult> : ISqlPage
    {
        public IProjection<TModel> M { get; }
        public IProjection<TResult> R { get; }
        public TModel Model { get; }

        public IProcContext Context { get; }
        public IProcRenderer Render => this.Context.Renderer;

        public ProcPage(IProjection model, IProjection result)
        {
            this.M = model?.Cast<TModel>() ?? throw new ArgumentNullException(nameof(model));
            this.R = result?.Cast<TResult>() ?? throw new ArgumentNullException(nameof(result));
            this.Context = model.Context;
            this.Model = (TModel)this.M.Attr().Field?.Invoke()?.Value;
        }

        public virtual void Execute() { }

        public void Write<T>(T value)
        {
            if (value is ISqlWritable w)
                w.WriteTo(this.Context.Execution.Buffer);
            else
            {
                ISchema schema = this.Context.Domain.Schemas.GetSchema(typeof(T));
                IField field = new Relation(value, schema);

                ProjectionIdentity identity = new ProjectionIdentity(schema, field);
                Projection projection = new Projection(identity, this.Context);

                this.Write(projection.Par());
            }
        }

        public void Write(object o) => this.Write<object>(o);
        public void WriteLiteral(string s) => this.Context.Execution.Buffer.Append(s);
    }
}