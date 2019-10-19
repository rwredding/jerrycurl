using Jerrycurl.Mvc.Test.Conventions.Accessors;
using Jerrycurl.Mvc.Test.Conventions.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Mvc.Test
{
    public class CrudTests
    {
        public void Create_WithLiterals_Works()
        {
            CrudAccessor crud = new CrudAccessor();

            crud.Run(@"CREATE TABLE IF NOT EXISTS CrudItem ( Id integer PRIMARY KEY AUTOINCREMENT, Counter integer NOT NULL, String nvarchar(50) NULL );");
            crud.Run(@"DELETE FROM CrudItem;");

            IList<CrudItem> newItems = Enumerable.Range(0, 20).Select(i => new CrudItem() { Counter = i, String = $"String {i}" }).ToList();

            crud.CreateWithLiterals(newItems);

            newItems.ShouldAllBe(it => it.Id > 0);
        }

        public void CreateReadUpdateDelete_Works()
        {
            CrudAccessor crud = new CrudAccessor();

            crud.Run(@"CREATE TABLE IF NOT EXISTS CrudItem ( Id integer PRIMARY KEY AUTOINCREMENT, Counter integer NOT NULL, String nvarchar(50) NULL );");
            crud.Run(@"DELETE FROM CrudItem;");

            IList<CrudItem> newItems = Enumerable.Range(0, 20).Select(i => new CrudItem() { Counter = i }).ToList();

            crud.Create(newItems);

            newItems.ShouldAllBe(it => it.Id > 0);

            foreach (CrudItem item in newItems)
                item.Counter *= 2;

            crud.Update(newItems);

            IList<CrudItem> getItems = crud.Get<CrudItem>();

            getItems.Select(it => it.Counter).ShouldBe(Enumerable.Range(0, 20).Select(i => i * 2));

            crud.Delete(getItems);

            crud.Get<CrudItem>().ShouldBeEmpty();
        }
    }
}
