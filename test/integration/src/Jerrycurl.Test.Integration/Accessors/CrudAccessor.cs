using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Test.Integration.Views;

namespace Jerrycurl.Test.Integration.Accessors
{
    public class CrudAccessor : Accessor
    {
        private string GetProcName(string procName)
        {
#if VENDOR_ORACLE
            return procName + ".Oracle";
#else
            return procName;
#endif
        }
        public IList<T> Get<T>() => this.Query<T>();
        public void Create<T>(T model) => this.Execute(model, commandName: this.GetProcName("Create"));
        public void Update<T>(T model) => this.Execute(model, commandName: this.GetProcName("Update"));
        public void Delete<T>(T model) => this.Execute(model, commandName: this.GetProcName("Delete"));
        public void Clear() => this.Execute(commandName: this.GetProcName("Clear"));
        public void VerifyImport()
        {
            var result = this.Query<int>();

            if (result.Count == 0)
                throw new InvalidOperationException();
        }

        public IList<DatabaseView> GetDatabaseView() => this.Query<DatabaseView>(queryName: this.GetProcName("GetDatabaseView"));
    }
}