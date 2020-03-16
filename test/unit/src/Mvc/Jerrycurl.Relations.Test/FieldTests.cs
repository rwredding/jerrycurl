using Jerrycurl.Relations.Tests.Models;
using Jerrycurl.Test;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Relations.Tests
{
    public class FieldTests
    {
        public void Test_Fields_HaveCorrectTypes()
        {
            RootModel model = new RootModel() { Complex = new RootModel.SubModel() };
            Relation rel = DatabaseHelper.Default.Relation(model, "", "Complex", "Complex.Complex", "Complex.Complex.Value");

            ITuple tuple = rel.Row();

            tuple[0].Type.ShouldBe(FieldType.Model);
            tuple[1].Type.ShouldBe(FieldType.Value);
            tuple[2].Type.ShouldBe(FieldType.Value);
            tuple[3].Type.ShouldBe(FieldType.Missing);
        }


        public void Test_Fields_EqualityImplementation()
        {
            RootModel model1 = new RootModel()
            {
                ComplexList = new List<RootModel.SubModel>()
                {
                    new RootModel.SubModel() { Value = 1 },
                    new RootModel.SubModel() { Value = 2 },
                }
            };
            RootModel model2 = new RootModel()
            {
                ComplexList = new List<RootModel.SubModel>()
                {
                    new RootModel.SubModel() { Value = 1 },
                    new RootModel.SubModel() { Value = 2 },
                }
            };

            Relation rel1_1 = DatabaseHelper.Default.Relation(model1, "ComplexList.Item.Value");
            Relation rel1_2 = DatabaseHelper.Default.Relation(model1, "ComplexList.Item.Value");
            Relation rel2_1 = DatabaseHelper.Default.Relation(model2, "ComplexList.Item.Value");

            IField[] fields1_1 = rel1_1.Column().ToArray();
            IField[] fields1_2 = rel1_2.Column().ToArray();
            IField[] fields2_1 = rel2_1.Column().ToArray();

            fields1_1.ShouldBe(fields1_2);
            fields1_1.ShouldNotBe(fields2_1);

            fields1_1.Select(f => f.Identity).ShouldBe(fields2_1.Select(f => f.Identity));
        }
    }
}
