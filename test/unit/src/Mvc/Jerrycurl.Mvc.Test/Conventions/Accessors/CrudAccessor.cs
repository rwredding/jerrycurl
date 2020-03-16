using Jerrycurl.Mvc.Test.Conventions.Models;
using System.Collections.Generic;

namespace Jerrycurl.Mvc.Test.Conventions.Accessors
{
    public class CrudAccessor : Accessor
    {
        public IList<T> Get<T>() => this.Query<T>();

        public void Create<T>(IEnumerable<T> model) => this.Execute(model);
        public void CreateWithLiterals<T>(IEnumerable<T> model) => this.Execute(model);
        public void Update<T>(IEnumerable<T> model) => this.Execute(model);
        public void Delete<T>(IEnumerable<T> model) => this.Execute(model);

        public void Run(Runnable runnable) => this.Execute(runnable);
        public void Run(string sql) => this.Run(new Runnable(sql));
    }
}
