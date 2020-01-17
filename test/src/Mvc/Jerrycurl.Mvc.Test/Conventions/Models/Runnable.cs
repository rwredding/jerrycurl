using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;
using System;
using System.Collections.Generic;

namespace Jerrycurl.Mvc.Test.Conventions.Models
{
    public class Runnable
    {
        private readonly List<ActionItem> actions = new List<ActionItem>();

        public Runnable()
        {

        }

        public Runnable(string sql)
            : this()
        {
            this.Literal(sql);
        }


        public void R(Func<IProjection, ISqlWritable> func) => this.actions.Add(new ActionItem() { Result = func });
        public void M(Func<IProjection, ISqlWritable> func) => this.actions.Add(new ActionItem() { Model = func });

        public void M<TModel>(Func<IProjection<TModel>, ISqlWritable> func) => this.M(p => func(p.Cast<TModel>()));
        public void R<TModel>(Func<IProjection<TModel>, ISqlWritable> func) => this.R(p => func(p.Cast<TModel>()));

        public void Literal(string sql) => this.actions.Add(new ActionItem() { Sql = sql });

        public void Execute<TModel, TResult>(ProcPage<TModel, TResult> page)
        {
            foreach (ActionItem a in this.actions)
            {
                if (a.Sql != null)
                    page.WriteLiteral(a.Sql);

                if (a.Result != null)
                    page.Write(a.Result(page.R));

                if (a.Model != null)
                    page.Write(a.Model(page.M));
            }
        }

        private class ActionItem
        {
            public string Sql { get; set; }
            public Func<IProjection, ISqlWritable> Result { get; set; }
            public Func<IProjection, ISqlWritable> Model { get; set; }
        }
    }
}
