using System.Collections.Generic;
using Jerrycurl.Extensions.EntityFrameworkCore.Test.Entities;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Shouldly;

namespace Jerrycurl.Extensions.EntityFrameworkCore.Test
{
    public class EntityTests
    {
        public void EnsureJoins_AreWorkingCorrectly()
        {
            Runnable<object, Order> table = new Runnable<object, Order>();

            table.Sql("SELECT ");
            table.Sql("1 AS "); table.R(p => p.Prop(m => m.Id));
            table.Sql(",1 AS "); table.R(p => p.Prop(m => m.BillingAddress.Id));
            table.Sql(",1 AS "); table.R(p => p.Prop(m => m.ShippingAddress.Id));
            table.Sql(" UNION ALL SELECT ");
            table.Sql("2 AS "); table.R(p => p.Prop(m => m.Id));
            table.Sql(",2 AS "); table.R(p => p.Prop(m => m.BillingAddress.Id));
            table.Sql(",NULL AS "); table.R(p => p.Prop(m => m.ShippingAddress.Id));

            table.Sql(";SELECT ");
            table.Sql("1 AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.Id));
            table.Sql(",1 AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.OrderId));
            table.Sql(",'Product 1' AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.Product));
            table.Sql(" UNION ALL SELECT ");
            table.Sql("2 AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.Id));
            table.Sql(",1 AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.OrderId));
            table.Sql(",'Product 2' AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.Product));
            table.Sql(" UNION ALL SELECT ");
            table.Sql("3 AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.Id));
            table.Sql(",1 AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.OrderId));
            table.Sql(",'Product 3' AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.Product));
            table.Sql(" UNION ALL SELECT ");
            table.Sql("4 AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.Id));
            table.Sql(",2 AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.OrderId));
            table.Sql(",'Product 1' AS "); table.R(p => p.Open(m => m.OrderLine).Prop(m => m.Product));

            IList<Order> orders = Runner.Query(table);

            orders.Count.ShouldBe(2);
            orders[0].BillingAddress.ShouldNotBeNull();
            orders[0].ShippingAddress.ShouldNotBeNull();
            orders[0].OrderLine.ShouldNotBeNull();
            orders[0].OrderLine.Count.ShouldBe(3);

            orders[1].BillingAddress.ShouldNotBeNull();
            orders[1].ShippingAddress.ShouldBeNull();
            orders[1].OrderLine.ShouldNotBeNull();
            orders[1].OrderLine.Count.ShouldBe(1);
        }
    }
}
