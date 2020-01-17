using Jerrycurl.Relations.Test.Models;
using Jerrycurl.Relations.Tests.Models;
using Jerrycurl.Test;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Relations.Tests
{
    public class RelationTests
    {
        public void Test_Reading_UnknownProperty_Throws()
        {
            Model model = new Model();
            Relation rel = DatabaseHelper.Default.Relation(model, "Unknown123");

            Should.Throw<RelationException>(() => rel.Scalar());
        }

        public void Test_Binding_ToModel_Throws()
        {
            Model model = new Model();
            Relation rel = DatabaseHelper.Default.Relation(model);

            Should.Throw<BindingException>(() => rel.Model.Bind(new Model()));
        }

        public void Test_Binding_ToMissing_Throws()
        {
            Model model = new Model();
            Relation rel = DatabaseHelper.Default.Relation(model, "Complex.Value");

            IField value = rel.Scalar();

            Should.Throw<BindingException>(() => value.Bind(10));
        }

        public void Test_Binding_ToReadOnlyProperty_Throws()
        {
            Relation rel = DatabaseHelper.Default.Relation(new Model(), "ReadOnly");

            Should.Throw<BindingException>(() => rel.Scalar().Bind(12));
        }

        public void Test_Binding_OfNonConvertibleValue_Throws()
        {
            Model model = new Model();
            Relation rel = DatabaseHelper.Default.Relation(model, "Complex.Value");

            IField value = rel.Scalar();

            Should.Throw<BindingException>(() => value.Bind("String"));
        }


        public void Test_Binding_NullToValueType_Throws()
        {
            Model model = new Model();
            Relation rel = DatabaseHelper.Default.Relation(model, "Complex.Value");

            IField value = rel.Scalar();

            Should.Throw<BindingException>(() => value.Bind(null));
        }


        public void Test_Binding_ToProperty()
        {
            Model model = new Model() { Complex = new Model.SubModel() };
            Relation rel = DatabaseHelper.Default.Relation(model, "Complex.Value");

            IField value = rel.Scalar();
            value.Bind(12);

            value.ShouldNotBeNull();
            value.Value.ShouldBe(12);
            model.Complex.Value.ShouldBe(12);
        }

        public void Test_Binding_ToNullValue()
        {
            Model model = new Model();
            Relation rel = DatabaseHelper.Default.Relation(model, "Complex");

            IField complex = rel.Scalar();

            complex.ShouldNotBeNull();

            Should.NotThrow(() => complex.Bind(new Model.SubModel() { Value = 10 }));

            model.Complex.Value.ShouldBe(10);
        }

        public void Test_Binding_ToListIndexer()
        {
            Model model = new Model() { IntList = new List<int>() { 1, 2, 3, 4, 5 } };
            Relation rel = DatabaseHelper.Default.Relation(model, "IntList.Item");

            Should.NotThrow(() => rel.Column().ElementAt(2).Bind(10));

            model.IntList.ShouldBe(new[] { 1, 2, 10, 4, 5 });
        }

        public void Test_Binding_ToEnumerableIndexer_Throws()
        {
            Model model = new Model() { IntEnumerable = new List<int>() { 1, 2, 3, 4, 5 } };
            Relation rel = DatabaseHelper.Default.Relation(model, "IntEnumerable.Item");

            Should.Throw<BindingException>(() => rel.Column().ElementAt(2).Bind(10));
        }

        public void Test_Binding_WithContravariance()
        {
            Model model = new Model();
            Relation rel = DatabaseHelper.Default.Relation(model, "Object");

            IField value = rel.Scalar();

            Should.NotThrow(() => value.Bind(new Model()));
        }

        public void Test_Binding_ToDeepObjectGraph()
        {
            Model model = new Model()
            {
                Complex = new Model.SubModel()
                {
                    Value = 50,
                    Complex = new Model.SubModel2()
                    {
                        Value = "String 1",
                    },
                },
                ComplexList = new List<Model.SubModel>()
                {
                    new Model.SubModel() { Complex = new Model.SubModel2() { Value = "String 2" } },
                    new Model.SubModel() { Complex = new Model.SubModel2() { Value = "String 3" } },
                },
            };
            Relation rel1 = DatabaseHelper.Default.Relation(model, "Complex.Value", "Complex.Complex.Value");
            Relation rel2 = DatabaseHelper.Default.Relation(model, "ComplexList.Item.Complex.Value");

            ITuple tuple1 = rel1.Row();
            IField[] tuple2 = rel2.Column().ToArray();

            tuple1[0].Bind(100);
            tuple1[1].Bind("String 3");
            tuple2[0].Bind("String 4");
            tuple2[1].Bind("String 5");


            model.Complex.Value.ShouldBe(100);
            model.Complex.Complex.Value.ShouldBe("String 3");
            model.ComplexList[0].Complex.Value.ShouldBe("String 4");
            model.ComplexList[1].Complex.Value.ShouldBe("String 5");
        }


        public void Test_Reading_OfDeepObjectGraphFromDifferentSources()
        {
            DeepModel model = new DeepModel()
            {
                Sub1 = new DeepModel.SubModel1()
                {
                    Sub2 = new DeepModel.SubModel2()
                    {
                        Sub3 = new List<DeepModel.SubModel3>()
                        {
                            new DeepModel.SubModel3()
                            {
                                Sub4 = new DeepModel.SubModel4()
                                {
                                    Sub5 = new List<DeepModel.SubModel5>()
                                    {
                                        new DeepModel.SubModel5()
                                        {
                                            Sub6 = new List<DeepModel.SubModel6>()
                                            {
                                                new DeepModel.SubModel6() { Value = 1 },
                                                new DeepModel.SubModel6() { Value = 2 },
                                                new DeepModel.SubModel6() { Value = 3 },
                                            },
                                        },
                                        new DeepModel.SubModel5()
                                        {
                                            Sub6 = new List<DeepModel.SubModel6>()
                                            {
                                                new DeepModel.SubModel6() { Value = 4 },
                                                new DeepModel.SubModel6() { Value = 5 },
                                            },
                                        },
                                        new DeepModel.SubModel5()
                                        {
                                            Sub6 = new List<DeepModel.SubModel6>()
                                            {
                                                new DeepModel.SubModel6() { Value = 6 },
                                            },
                                        }
                                    },
                                },
                            },
                            new DeepModel.SubModel3()
                            {
                                Sub4 = new DeepModel.SubModel4()
                                {
                                    Sub5 = new List<DeepModel.SubModel5>()
                                    {
                                        new DeepModel.SubModel5()
                                        {
                                            Sub6 = new List<DeepModel.SubModel6>()
                                            {
                                                new DeepModel.SubModel6() { Value = 7 },
                                                new DeepModel.SubModel6() { Value = 8 },
                                                null,
                                            },
                                        },
                                        new DeepModel.SubModel5()
                                        {
                                            Sub6 = new List<DeepModel.SubModel6>()
                                            {
                                                new DeepModel.SubModel6() { Value = 9 },
                                            },
                                        },
                                    },
                                },
                            },
                            new DeepModel.SubModel3()
                            {
                                Sub4 = new DeepModel.SubModel4()
                                {
                                    Sub5 = new List<DeepModel.SubModel5>(),
                                },
                            },
                        },
                    },
                },
            };

            string valueAttr = "Sub1.Sub2.Sub3.Item.Sub4.Sub5.Item.Sub6.Item.Value";

            Relation rel1 = DatabaseHelper.Default.Relation(model, valueAttr);
            Relation rel2 = new Relation(new Relation(rel1, "Sub1").Scalar(), valueAttr);
            Relation rel3 = new Relation(new Relation(rel2, "Sub1.Sub2").Scalar(), valueAttr);
            Relation rel4 = new Relation(new Relation(rel3, "Sub1.Sub2.Sub3").Scalar(), valueAttr);
            Relation rel5 = new Relation(new Relation(rel4, "Sub1.Sub2.Sub3.Item").Scalar(), valueAttr);
            Relation rel6 = new Relation(new Relation(rel5, "Sub1.Sub2.Sub3.Item.Sub4").Scalar(), valueAttr);
            Relation rel7 = new Relation(new Relation(rel6, "Sub1.Sub2.Sub3.Item.Sub4.Sub5").Scalar(), valueAttr);
            Relation rel8 = new Relation(new Relation(rel7, "Sub1.Sub2.Sub3.Item.Sub4.Sub5.Item").Scalar(), valueAttr);
            Relation rel9 = new Relation(new Relation(rel8, "Sub1.Sub2.Sub3.Item.Sub4.Sub5.Item.Sub6").Scalar(), valueAttr);
            Relation rel10 = new Relation(new Relation(rel9, "Sub1.Sub2.Sub3.Item.Sub4.Sub5.Item.Sub6.Item").Scalar(), valueAttr);
            Relation rel11 = new Relation(new Relation(rel10, "Sub1.Sub2.Sub3.Item.Sub4.Sub5.Item.Sub6.Item.Value").Scalar(), valueAttr);

            rel1.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, null, 9 });
            rel2.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, null, 9 });
            rel3.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, null, 9 });
            rel4.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, null, 9 });
            rel5.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3, 4, 5, 6 });
            rel6.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3, 4, 5, 6 });
            rel7.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3, 4, 5, 6 });
            rel8.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3 });
            rel9.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1, 2, 3 });
            rel10.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1 });
            rel11.Column().Select(f => (int?)f.Value).ShouldBe(new int?[] { 1 });
        }

        public void Test_Reading_OneToOneWithOuterJoin()
        {
            Model model = new Model() { IntValue = 1 };
            ITuple tuple = DatabaseHelper.Default.Relation(model, "IntValue", "Complex.Complex.Value").Row();

            tuple.Degree.ShouldBe(2);

            tuple[0].Value.ShouldBe(1);
            tuple[1].Value.ShouldBeNull();
        }

        public void Test_Reading_OneToManyWithInnerJoin()
        {
            List<Model> model = new List<Model>()
            {
                new Model()
                {
                    IntValue = 1,
                    ComplexList = new List<Model.SubModel>()
                    {
                        new Model.SubModel() { Value = 10 }
                    }
                },
                new Model()
                {
                    IntValue = 2,
                    ComplexList = new List<Model.SubModel>()
                    {
                        new Model.SubModel() { Value = 11 },
                        new Model.SubModel() { Value = 12 }
                    }
                },
                new Model() { IntValue = 3 },
                new Model() { IntValue = 4, ComplexList = new List<Model.SubModel>() },
            };

            ITuple[] result = DatabaseHelper.Default.Relation(model, "Item.IntValue", "Item.ComplexList.Item.Value").ToArray();

            result.Length.ShouldBe(3);

            result[0][0].Value.ShouldBe(1);
            result[0][1].Value.ShouldBe(10);

            result[1][0].Value.ShouldBe(2);
            result[1][1].Value.ShouldBe(11);

            result[2][0].Value.ShouldBe(2);
            result[2][1].Value.ShouldBe(12);
        }

        public void Test_Reading_AdjacentListsWithCrossJoin()
        {
            Model model = new Model()
            {
                ComplexList = new List<Model.SubModel>()
                {
                    new Model.SubModel() { Value = 1 },
                    new Model.SubModel() { Value = 2 },
                    new Model.SubModel() { Value = 3 },
                },
                ComplexList2 = new List<Model.SubModel>()
                {
                    new Model.SubModel() { Value = 4 },
                    new Model.SubModel() { Value = 5 },
                    new Model.SubModel() { Value = 6 },
                    new Model.SubModel() { Value = 7 },
                }
            };
            Relation rel1 = DatabaseHelper.Default.Relation(model, "ComplexList.Item.Value", "ComplexList2.Item.Value");
            Relation rel2 = DatabaseHelper.Default.Relation(model, "ComplexList2.Item.Value", "ComplexList.Item.Value");

            IList<(int, int)> pairs1 = rel1.Select(t => ((int)t[0].Value, (int)t[1].Value)).ToList();
            IList<(int, int)> pairs2 = rel2.Select(t => ((int)t[0].Value, (int)t[1].Value)).ToList();

            pairs1.Count.ShouldBe(3 * 4);
            pairs2.Count.ShouldBe(4 * 3);

            pairs1[0].ShouldBe((1, 4));
            pairs1[1].ShouldBe((1, 5));
            pairs1[2].ShouldBe((1, 6));
            pairs1[3].ShouldBe((1, 7));
            pairs1[4].ShouldBe((2, 4));
            pairs1[5].ShouldBe((2, 5));
            pairs1[6].ShouldBe((2, 6));
            pairs1[7].ShouldBe((2, 7));
            pairs1[8].ShouldBe((3, 4));
            pairs1[9].ShouldBe((3, 5));
            pairs1[10].ShouldBe((3, 6));
            pairs1[11].ShouldBe((3, 7));

            pairs2[0].ShouldBe((4, 1));
            pairs2[1].ShouldBe((4, 2));
            pairs2[2].ShouldBe((4, 3));
            pairs2[3].ShouldBe((5, 1));
            pairs2[4].ShouldBe((5, 2));
            pairs2[5].ShouldBe((5, 3));
            pairs2[6].ShouldBe((6, 1));
            pairs2[7].ShouldBe((6, 2));
            pairs2[8].ShouldBe((6, 3));
            pairs2[9].ShouldBe((7, 1));
            pairs2[10].ShouldBe((7, 2));
            pairs2[11].ShouldBe((7, 3));
        }

        public void Test_Reading_ScalarList()
        {
            Model model = new Model()
            {
                IntList = new List<int>() { 1, 2, 3, 4, 5 },
            };
            Relation rel = DatabaseHelper.Default.Relation(model, "IntList.Item");

            IEnumerable<int> ints = rel.Column().Select(f => (int)f.Value);

            ints.ShouldBe(new[] { 1, 2, 3, 4, 5 });
        }

        public void Test_Reading_OfValueFromNonParentSource_Throws()
        {
            Model model = new Model()
            {
                IntValue = 100,
                IntList = new List<int>(),
            };
            Relation rel1 = DatabaseHelper.Default.Relation(model, "IntValue");

            IField intValue = rel1.Scalar();

            Relation rel2 = Should.NotThrow(() => new Relation(intValue, "IntList"));
            Should.Throw<RelationException>(() => rel2.Scalar());
        }

    }
}
