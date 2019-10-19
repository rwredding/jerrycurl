using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Data.Commands;
using Microsoft.Data.Sqlite;
using Shouldly;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries;
using Jerrycurl.Data.Test.Models;
using Jerrycurl.Test;

namespace Jerrycurl.Data.Test
{
    public class QueryTests
    {
#if NETCOREAPP3_0
        public async Task Test_Binding_OfEnumerateAsyncWithMultipleSets()
        {
            SqliteTable table1 = new SqliteTable("Item")
            {
                new object[] { 1 },
                new object[] { 2 },
                new object[] { 3 },
            };
            SqliteTable table2 = new SqliteTable("Item")
            {
                new object[] { 4 },
                new object[] { 5 },
                new object[] { 6 },
            };

            DatabaseHelper.Default.Enumerate<int>(table1, table2).ShouldBe(new[] { 1, 2, 3, 4, 5, 6 });
            (await (DatabaseHelper.Default.EnumerateAsync<int>(table1, table2)).ToList()).ShouldBe(new[] { 1, 2, 3, 4, 5, 6 });
        }
#endif

        public async Task Test_Binding_OfNestedStructs()
        {
            SqliteTable table = new SqliteTable("Item.Integer", "Item.String", "Item.Sub.Value")
            {
                new object[] { 1, "Jerrycurl", 2 },
            };

            BigStruct result1 = DatabaseHelper.Default.Query<BigStruct>(table).FirstOrDefault();
            BigStruct result2 = (await DatabaseHelper.Default.QueryAsync<BigStruct>(table)).FirstOrDefault();

            result1.Integer.ShouldBe(1);
            result2.Integer.ShouldBe(1);

            result1.String.ShouldBe("Jerrycurl");
            result2.String.ShouldBe("Jerrycurl");

            result1.Sub.Value.ShouldBe(2);
            result2.Sub.Value.ShouldBe(2);
        }

        public async Task Test_Binding_OfResultSetWithoutColumns()
        {
            QueryData query = new QueryData()
            {
                QueryText = @"CREATE TABLE IF NOT EXISTS Temp001 ( Id integer );
                              DROP TABLE Temp001;",
            };

            IList<BigModel> result1 = DatabaseHelper.Default.Query<BigModel>(query);
            IList<BigModel> result2 = (await DatabaseHelper.Default.QueryAsync<BigModel>(query));
            IList<BigModel> result3 = DatabaseHelper.Default.Enumerate<BigModel>(query).ToList();

            result1.ShouldBeEmpty();
            result2.ShouldBeEmpty();
            result3.ShouldBeEmpty();
        }

        public async Task Test_Binding_OfResultSetWithoutMatchingColumns()
        {
            SqliteTable table = new SqliteTable("X")
            {
                new object[] { 1 },
                new object[] { 2 }
            };

            IList<BigAggregate> result1 = DatabaseHelper.Default.Query<BigAggregate>(table);
            IList<BigAggregate> result2 = await DatabaseHelper.Default.QueryAsync<BigAggregate>(table);
            IList<BigModel> result3 = DatabaseHelper.Default.Query<BigModel>(table);
            IList<BigModel> result4 = await DatabaseHelper.Default.QueryAsync<BigModel>(table);
            IList<BigModel> result5 = DatabaseHelper.Default.Enumerate<BigModel>(table).ToList();

            result1.ShouldBeEmpty();
            result2.ShouldBeEmpty();
            result3.ShouldBeEmpty();
            result4.ShouldBeEmpty();
            result5.ShouldBe(new[] { (BigModel)null, null });
        }

        public async Task Test_Binding_OfValuesFromNullableParameters()
        {
            QueryData query = new QueryData()
            {
                QueryText = @"SELECT @P0 AS `Item` UNION
                              SELECT @P1 AS `Item` UNION
                              SELECT @P2 AS `Item`
                              ORDER BY `Item`",
                Parameters = new IParameter[]
                {
                    new Parameter("P0", DatabaseHelper.Default.Field<int?>(0)),
                    new Parameter("P1", DatabaseHelper.Default.Field<int?>()),
                    new Parameter("P2", DatabaseHelper.Default.Field<int?>(1)),
                }
            };

            IList<int?> result1 = DatabaseHelper.Default.Query<int?>(query);
            IList<int?> result2 = await DatabaseHelper.Default.QueryAsync<int?>(query);

            static void verifyResult(IList<int?> result)
            {
                result.ShouldNotBeNull();
                result.ShouldBe(new int?[] { null, 0, 1 });
            }

            verifyResult(result1);
            verifyResult(result2);
        }

        public async Task Test_Binding_OfBigAggregateResult()
        {
            SqliteTable table2 = new SqliteTable("Item.None.BigKey", "Item.None.Value")
            {
                new object[] { null, 1 },
            };
            SqliteTable table1 = new SqliteTable("Item.Scalar")
            {
                new object[] { 2 },
                new object[] { 1 },
            };
            SqliteTable table3 = new SqliteTable("Item.One.BigKey", "Item.One.Value")
            {
                new object[] { 1, 22 },
            };
            SqliteTable table4 = new SqliteTable("Item.Many.Item.BigKey", "Item.Many.Item.Value", "Item.Many.Item.OneToMany.Item.BigKey", "Item.Many.Item.OneToMany.Item.Value")
            {
                new object[] { 1, 33, 3, 22 },
                new object[] { null, 34, null, 23 },
                new object[] { 3, 35, 3, 24 },
            };

            IList<BigAggregate> result1 = DatabaseHelper.Default.Query<BigAggregate>(table1, table2, table3, table4);
            IList<BigAggregate> result2 = await DatabaseHelper.Default.QueryAsync<BigAggregate>(table1, table2, table3, table4);

            static void verifyResult(IList<BigAggregate> result)
            {
                result.ShouldNotBeNull();
                result.Count.ShouldBe(1);

                result[0].Scalar.ShouldBe(2);
                result[0].None.ShouldBeNull();
                result[0].One.ShouldNotBeNull();
                result[0].One.Value.ShouldBe(22);

                result[0].Many.ShouldNotBeNull();
                result[0].Many.Select(m => m.Value).ShouldBe(new[] { 33, 35 });

                result[0].Many[0].OneToMany.ShouldBeEmpty();
                result[0].Many[1].OneToMany.ShouldNotBeNull();
                result[0].Many[1].OneToMany.Select(m => m.Value).ShouldBe(new[] { 22, 24 });
            }

            verifyResult(result1);
            verifyResult(result2);
        }

        public async Task Test_Binding_ToReadOnlyProperty_Throws()
        {
            SqliteTable table1 = new SqliteTable("Item.ReadOnly")
            {
                new object[] { 1 },
            };

            Should.Throw<BindingException>(() => DatabaseHelper.Default.Query<BigModel>(table1));
            await Should.ThrowAsync<BindingException>(async () => await DatabaseHelper.Default.QueryAsync<BigModel>(table1));
        }

        public async Task Test_Binding_OfNonConvertibleValue_Throws()
        {
            SqliteTable table1 = new SqliteTable("Item.Value")
            {
                new object[] { "String 0" },
            };

            Should.Throw<BindingException>(() => DatabaseHelper.Default.Query<BigModel>(table1));
            await Should.ThrowAsync<BindingException>(async () => await DatabaseHelper.Default.QueryAsync<BigModel>(table1));
        }

        public async Task Test_Binding_OfSimpleProperties()
        {
            SqliteTable table1 = new SqliteTable("Item.Value", "Item.Value2")
            {
                new object[] { 1, "String 1" },
                new object[] { 2, "String 2" },
            };

            IList<BigModel> result1 = DatabaseHelper.Default.Query<BigModel>(table1);
            IList<BigModel> result2 = await DatabaseHelper.Default.QueryAsync<BigModel>(table1);
            IList<BigModel> result3 = DatabaseHelper.Default.Enumerate<BigModel>(table1).ToList();

            static void verifyResult(IList<BigModel> result)
            {
                result.ShouldNotBeNull();

                result.Count.ShouldBe(2);

                result[0].Value.ShouldBe(1);
                result[0].Value2.ShouldBe("String 1");

                result[1].Value.ShouldBe(2);
                result[1].Value2.ShouldBe("String 2");
            }

            verifyResult(result1);
            verifyResult(result2);
            verifyResult(result3);
        }

        public async Task Test_Binding_OfScalarNullIntResult()
        {
            SqliteTable table1 = new SqliteTable("Item")
            {
                new object[] { 1 },
                new object[] { null },
                new object[] { 2 },
            };

            DatabaseHelper.Default.Query<int?>(table1).ShouldBe(new int?[] { 1, null, 2 });
            DatabaseHelper.Default.Enumerate<int?>(table1).ShouldBe(new int?[] { 1, null, 2 });
            (await DatabaseHelper.Default.QueryAsync<int?>(table1)).ShouldBe(new int?[] { 1, null, 2 });
        }

        public async Task Test_Binding_OfDynamicScalarIntResult()
        {
            SqliteTable table1 = new SqliteTable("Item")
            {
                new object[] { 1 },
                new object[] { 2 },
                new object[] { null },
            };

            IList<dynamic> result1 = DatabaseHelper.Default.Query<dynamic>(table1);
            IList<dynamic> result2 = await DatabaseHelper.Default.QueryAsync<dynamic>(table1);
            IList<dynamic> result3 = DatabaseHelper.Default.Enumerate<dynamic>(table1).ToList();

            static void verifyResult(IList<dynamic> result)
            {
                result.ShouldNotBeNull();
                result.Cast<long?>().ShouldBe(new long?[] { 1, 2, null });
            }

            verifyResult(result1);
            verifyResult(result2);
            verifyResult(result3);
        }

        public async Task Test_Binding_OfDynamicProperties()
        {
            SqliteTable table1 = new SqliteTable("Item.Value1", "Item.Value2", "Item.Sub.Value3")
            {
                new object[] { 1, 2,    9 },
                new object[] { 3, null, 8 },
            };

            IList<dynamic> result1 = DatabaseHelper.Default.Query<dynamic>(table1);
            IList<dynamic> result2 = await DatabaseHelper.Default.QueryAsync<dynamic>(table1);
            IList<dynamic> result3 = DatabaseHelper.Default.Enumerate<dynamic>(table1).ToList();

            static void verifyResult(IList<dynamic> result)
            {
                result.ShouldNotBeNull();
                result.Count.ShouldBe(2);

                DynamicShould.HaveProperty(result[0], "Value1");
                DynamicShould.HaveProperty(result[0], "Value2");
                DynamicShould.HaveProperty(result[0], "Sub");
                DynamicShould.HaveProperty(result[0].Sub, "Value3");

                DynamicShould.HaveProperty(result[1], "Value1");
                DynamicShould.HaveProperty(result[1], "Value2");
                DynamicShould.HaveProperty(result[1], "Sub");
                DynamicShould.HaveProperty(result[1].Sub, "Value3");

                int v1 = Should.NotThrow(() => (int)result[0].Value1);
                int v2 = Should.NotThrow(() => (int)result[0].Value2);
                int v3 = Should.NotThrow(() => (int)result[0].Sub.Value3);
                int v4 = Should.NotThrow(() => (int)result[1].Value1);
                int? v5 = Should.NotThrow(() => (int?)result[1].Value2);
                int? v6 = Should.NotThrow(() => (int?)result[1].Sub.Value3);

                v1.ShouldBe(1);
                v2.ShouldBe(2);
                v3.ShouldBe(9);
                v4.ShouldBe(3);
                v5.ShouldBe(null);
                v6.ShouldBe(8);
            }

            verifyResult(result1);
            verifyResult(result2);
            verifyResult(result3);
        }

        public async Task Test_Binding_OfNativeManyType()
        {
            SqliteTable table1 = new SqliteTable("Item.BigKey")
            {
                new object[] { 1 },
                new object[] { 2 },
                new object[] { 3 },
            };
            SqliteTable table2 = new SqliteTable("Item.ManyType.Item.BigKey", "Item.ManyType.Item.Value")
            {
                new object[] { 1, 3 },
                new object[] { 1, 4 },
                new object[] { 2, 5 },
            };

            IList<BigModel> result1 = DatabaseHelper.Default.Query<BigModel>(table1, table2);
            IList<BigModel> result2 = await DatabaseHelper.Default.QueryAsync<BigModel>(table1, table2);

            static void verifyResult(IList<BigModel> result)
            {
                result.ShouldNotBeNull();
                result.Count.ShouldBe(3);

                result[0].ManyType.HasValue.ShouldBeTrue();
                result[0].ManyType.Value.ShouldNotBeNull();
                result[0].ManyType.Value.Value.ShouldBe(3);

                result[1].ManyType.HasValue.ShouldBeTrue();
                result[1].ManyType.Value.ShouldNotBeNull();
                result[1].ManyType.Value.Value.ShouldBe(5);

                result[2].ManyType.HasValue.ShouldBeFalse();
                result[2].ManyType.Value.ShouldBeNull();
            }

            verifyResult(result1);
            verifyResult(result2);
        }
        public async Task Test_Binding_OfOneToOneSelfJoins()
        {
            SqliteTable table1 = new SqliteTable("Item.Parent.Id", "Item.Parent.ParentId")
            {
                new object[] { 4,    null },
                new object[] { 3,    4 },
                new object[] { 2,    3 },
            };
            SqliteTable table2 = new SqliteTable("Item.Id", "Item.ParentId")
            {
                new object[] { 1, 2 },
            };

            IList<BigRecurse.One> result1 = DatabaseHelper.Default.Query<BigRecurse.One>(table1, table2);
            IList<BigRecurse.One> result2 = await DatabaseHelper.Default.QueryAsync<BigRecurse.One>(table1, table2);

            static void verifyResult(IList<BigRecurse.One> result)
            {
                result.ShouldNotBeNull();
                result.Count.ShouldBe(1);

                result[0].Id.ShouldBe(1);
                result[0].Parent.ShouldNotBeNull();

                result[0].Parent.Id.ShouldBe(2);
                result[0].Parent.Parent.ShouldNotBeNull();

                result[0].Parent.Parent.Id.ShouldBe(3);
                result[0].Parent.Parent.Parent.ShouldNotBeNull();

                result[0].Parent.Parent.Parent.Id.ShouldBe(4);
                result[0].Parent.Parent.Parent.Parent.ShouldBeNull();
            }

            verifyResult(result1);
            verifyResult(result2);
        }

        public async Task Test_Binding_OfBigModelWithDifferentJoins()
        {
            SqliteTable table1 = new SqliteTable("Item.OneToManyAsOne.BigKey", "Item.OneToManyAsOne.Value")
            {
                new object[] { 3, 99 },
                new object[] { 2, 999 },
            };
            SqliteTable table2 = new SqliteTable("Item.BigKey", "Item.Value", "Item.OneToOne.SubKey", "Item.OneToOne.Value")
            {
                new object[] { 1, 77, 1, 1 },
                new object[] { 2, 777, null, 2 },
            };
            SqliteTable table3 = new SqliteTable("Item.BigKey", "Item.Value", "Item.OneToOne.Value")
            {
                new object[] { 3, 7777, 3 },
            };
            SqliteTable table4 = new SqliteTable("Item.OneToMany.Item.BigKey", "Item.OneToMany.Item.Value")
            {
                new object[] { 2, 55 },
                new object[] { 2, 555 },
                new object[] { 3, 66 },
            };
            SqliteTable table5 = new SqliteTable("Item.OneToManySelf.Item.BigKey", "Item.OneToManySelf.Item.Id", "Item.OneToManySelf.Item.ParentId")
            {
                new object[] { 1, 1, null },
                new object[] { 1, 2, null },
                new object[] { 1, 3, null },
                new object[] { 2, 4, null },
            };
            SqliteTable table6 = new SqliteTable("Item.OneToManySelf.Item.Children.Item.Id", "Item.OneToManySelf.Item.Children.Item.ParentId")
            {
                new object[] { 5, 2 },
                new object[] { 6, 2 },
                new object[] { 7, 3 },
                new object[] { 8, 3 },
                new object[] { 9, 3 },
                new object[] { 10, 6 },
                new object[] { 11, 6 },
                new object[] { 12, 9 },
            };

            IList<BigModel> result1 = DatabaseHelper.Default.Query<BigModel>(table1, table2, table3, table4, table5, table6);
            IList<BigModel> result2 = await DatabaseHelper.Default.QueryAsync<BigModel>(table1, table2, table3, table4, table5, table6);

            static void verifyResult(IList<BigModel> result)
            {
                result.ShouldNotBeNull();
                result.Select(m => m.Value).ShouldBe(new[] { 77, 777, 7777 });

                result[0].OneToOne.ShouldNotBeNull();
                result[0].OneToOne.Value.ShouldBe(1);
                result[1].OneToOne.ShouldBeNull();
                result[2].OneToOne.ShouldNotBeNull();
                result[2].OneToOne.Value.ShouldBe(3);

                result[0].OneToManyAsOne.ShouldBeNull();
                result[1].OneToManyAsOne.ShouldNotBeNull();
                result[1].OneToManyAsOne.Value.ShouldBe(999);
                result[2].OneToManyAsOne.ShouldNotBeNull();
                result[2].OneToManyAsOne.Value.ShouldBe(99);

                result[0].OneToMany.ShouldNotBeNull();
                result[0].OneToMany.ShouldBeEmpty();
                result[1].OneToMany.ShouldNotBeNull();
                result[1].OneToMany.Select(m => m.Value).ShouldBe(new[] { 55, 555 });
                result[2].OneToMany.ShouldNotBeNull();
                result[2].OneToMany.Select(m => m.Value).ShouldBe(new[] { 66 });

                result[0].OneToManySelf.ShouldNotBeNull();
                result[0].OneToManySelf.Select(m => m.Id).ShouldBe(new[] { 1, 2, 3 });
                result[1].OneToManySelf.ShouldNotBeNull();
                result[1].OneToManySelf.Select(m => m.Id).ShouldBe(new[] { 4 });
                result[2].OneToManySelf.ShouldBeEmpty();

                result[0].OneToManySelf[0].Children.ShouldBeEmpty();
                result[0].OneToManySelf[1].Children.Select(m => m.Id).ShouldBe(new[] { 5, 6 });
                result[0].OneToManySelf[2].Children.Select(m => m.Id).ShouldBe(new[] { 7, 8, 9 });

                result[0].OneToManySelf[1].Children[1].Children.Select(m => m.Id).ShouldBe(new[] { 10, 11 });
                result[0].OneToManySelf[2].Children[2].Children.Select(m => m.Id).ShouldBe(new[] { 12 });
            };

            verifyResult(result1);
            verifyResult(result2);
        }

        public async Task Test_Binding_OfAggregateWithEmptySets()
        {
            QueryData query = new QueryData()
            {
                QueryText = @"SELECT 1 AS `Item.NotUsedOne.Value` FROM sqlite_master WHERE 0 = 1;
                              SELECT 1 AS `Item.NotUsedMany.Item.Value` FROM sqlite_master WHERE 0 = 1",
            };

            IList<BigAggregate> result1 = DatabaseHelper.Default.Query<BigAggregate>(query);
            IList<BigAggregate> result2 = await DatabaseHelper.Default.QueryAsync<BigAggregate>(query);

            result1.ShouldNotBeEmpty();
            result2.ShouldNotBeEmpty();

            result1.Count.ShouldBe(1);
            result2.Count.ShouldBe(1);

            result1[0].NotUsedOne.ShouldBeNull();
            result2[0].NotUsedOne.ShouldBeNull();

            result1[0].NotUsedMany.ShouldBeEmpty();
            result2[0].NotUsedMany.ShouldBeEmpty();
        }

        public async Task Test_Binding_UsingHashJoinsInDifferentSets()
        {
            SqliteTable table1 = new SqliteTable("Item.BigKey", "Item.OneToMany.Item.BigKey", "Item.OneToMany.Item.Value")
            {
                new object[] { 1, 2, 2 },
                new object[] { 2, null, null }
            };
            SqliteTable table2 = new SqliteTable("Item.OneToMany.Item.BigKey", "Item.OneToMany.Item.Value")
            {
                new object[] { 1, 1 },
                new object[] { 2, 3 },
            };

            IList<BigModel> result1 = DatabaseHelper.Default.Query<BigModel>(table1, table2);
            IList<BigModel> result2 = await DatabaseHelper.Default.QueryAsync<BigModel>(table1, table2);

            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();

            result1.Count.ShouldBe(2);
            result2.Count.ShouldBe(2);

            result1[0].OneToMany.ShouldNotBeNull();
            result1[0].OneToMany.Count.ShouldBe(1);
            result1[0].OneToMany[0].ShouldNotBeNull();
            result1[0].OneToMany[0].Value.ShouldBe(1);
            result2[0].OneToMany.ShouldNotBeNull();
            result2[0].OneToMany.Count.ShouldBe(1);
            result2[0].OneToMany[0].ShouldNotBeNull();
            result2[0].OneToMany[0].Value.ShouldBe(1);

            result1[1].OneToMany.ShouldNotBeNull();
            result1[1].OneToMany.Count.ShouldBe(2);
            result1[1].OneToMany[0].ShouldNotBeNull();
            result1[1].OneToMany[0].Value.ShouldBe(2);
            result1[1].OneToMany[1].ShouldNotBeNull();
            result1[1].OneToMany[1].Value.ShouldBe(3);
            result2[1].OneToMany.ShouldNotBeNull();
            result2[1].OneToMany.Count.ShouldBe(2);
            result2[1].OneToMany[0].ShouldNotBeNull();
            result2[1].OneToMany[0].Value.ShouldBe(2);
            result2[1].OneToMany[1].ShouldNotBeNull();
            result2[1].OneToMany[1].Value.ShouldBe(3);
        }

        public async Task Test_Binding_OfResultsWithoutNullKeys()
        {
            SqliteTable table = new SqliteTable("Item.Value")
            {
                new object[] { 1 },
                new object[] { null },
            };

            IList<BigModel> result1 = DatabaseHelper.Default.Query<BigModel>(table);
            IList<BigModel> result2 = (await DatabaseHelper.Default.QueryAsync<BigModel>(table));
            IList<BigModel> result3 = DatabaseHelper.Default.Enumerate<BigModel>(table).ToList();

            result1.Count.ShouldBe(2);
            result2.Count.ShouldBe(2);
            result3.Count.ShouldBe(2);

            result1[0].ShouldNotBeNull();
            result1[1].ShouldNotBeNull();
            result2[0].ShouldNotBeNull();
            result2[1].ShouldNotBeNull();
            result3[0].ShouldNotBeNull();
            result3[1].ShouldNotBeNull();
        }

        public async Task Test_Binding_OfResultsWithNullKeys()
        {
            SqliteTable table = new SqliteTable("Item.BigKey", "Item.Value")
            {
                new object[] { null, 1 },
                new object[] { 10, 1 },
            };

            IList<BigModel> result1 = DatabaseHelper.Default.Query<BigModel>(table);
            IList<BigModel> result2 = (await DatabaseHelper.Default.QueryAsync<BigModel>(table));
            IList<BigModel> result3 = DatabaseHelper.Default.Enumerate<BigModel>(table).ToList();

            result1.Count.ShouldBe(1);
            result2.Count.ShouldBe(1);
            result3.Count.ShouldBe(2);

            result1[0].ShouldNotBeNull();
            result2[0].ShouldNotBeNull();
            result3[0].ShouldBeNull();
            result3[1].ShouldNotBeNull();
        }

        public void Test_Binding_OfCaseInsensitiveColumns()
        {
            SqliteTable table = new SqliteTable("item.value")
            {
                new object[] { 22 },
            };

            BigModel result = DatabaseHelper.Default.Query<BigModel>(table).FirstOrDefault();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(22);
        }

        public void Test_Binding_OfEmptyParameters()
        {
            QueryData query = new QueryData()
            {
                QueryText = "SELECT CASE WHEN @P1 IS NULL THEN 12 ELSE 0 END AS Item",
                Parameters = new[]
                {
                    new Parameter("@P1"),
                }
            };

            IList<int> result = DatabaseHelper.Default.Query<int>(query);

            result.ShouldBe(new[] { 12 });
        }

    }
}
