using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.Relations;
using Jerrycurl.Data.Commands;
using Shouldly;
using Jerrycurl.Data.Test.Models;
using Jerrycurl.Test;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Test
{
    public class CommandTests
    {
        public async Task Test_Execute_WithCaseInsensitiveColumns()
        {
            IList<int> personIds = new List<int>() { 0 };

            IField field = DatabaseHelper.Default.Relation(personIds, "Item").Scalar();

            CommandData command = new CommandData()
            {
                CommandText = "SELECT 1 AS b1",
                Bindings = new ICommandBinding[]
                {
                    new ColumnBinding("B1", field),
                },
            }; ;

            DatabaseHelper.Default.Execute(command);

            personIds.ShouldBe(new[] { 1 });

            personIds[0] = 0;

            await DatabaseHelper.Default.ExecuteAsync(command);

            personIds.ShouldBe(new[] { 1 });
        }

        public async Task Test_Execute_WithParameterPropagationBetweenCommands()
        {
            IList<int> personIds = new List<int>() { 0, 0 };

            IField[] fields = DatabaseHelper.Default.Relation(personIds, "Item").Column().ToArray();

            CommandData[] commands = new CommandData[]
            {
                new CommandData()
                {
                    CommandText = "SELECT 1 AS B1",
                    Bindings = new ICommandBinding[]
                    {
                        new ColumnBinding("B1", fields[0]),
                    },
                    Parameters = new IParameter[]
                    {
                        new Parameter("P1", fields[0]),
                    }
                },
                new CommandData()
                {
                    CommandText = "SELECT @P1 * 2 AS B2",
                    Bindings = new ICommandBinding[]
                    {
                        new ColumnBinding("B2", fields[1]),
                    },
                    Parameters = new IParameter[]
                    {
                        new Parameter("P1", fields[0]),
                    }
                }
            };

            DatabaseHelper.Default.Execute(commands);

            personIds.ShouldBe(new[] { 1, 2 });

            personIds[0] = 0;
            personIds[1] = 0;

            await DatabaseHelper.Default.ExecuteAsync(commands);

            personIds.ShouldBe(new[] { 1, 2 });
        }

        public async Task Test_Execute_WithColumnBindingToMissingValue_Throws()
        {
            BigModel model = new BigModel();

            IField field = DatabaseHelper.Default.Relation(model, "OneToOne.Value").Scalar();

            CommandData command = new CommandData()
            {
                CommandText = @"SELECT 12 AS B1",
                Bindings = new ICommandBinding[]
                {
                    new ColumnBinding("B1", field),
                }
            };

            Should.Throw<Relations.BindingException>(() => DatabaseHelper.Default.Execute(command));
            await Should.ThrowAsync<Relations.BindingException>(async () => await DatabaseHelper.Default.ExecuteAsync(command));
        }

        public async Task Test_Execute_WithColumnBindingToProperty()
        {
            BigModel model1 = new BigModel() { Value = 1, Value2 = "banana" };
            BigModel model2 = new BigModel() { Value = 1, Value2 = "banana" };

            ITuple tuple1 = DatabaseHelper.Default.Relation(model1, "Value", "Value2").Row();
            ITuple tuple2 = DatabaseHelper.Default.Relation(model2, "Value", "Value2").Row();

            CommandData command1 = new CommandData()
            {
                CommandText = @"SELECT 2 AS B1, 'apple' AS B2;",
                Bindings = new ICommandBinding[]
                {
                    new ColumnBinding("B1", tuple1[0]),
                    new ColumnBinding("B2", tuple1[1]),
                }
            };
            CommandData command2 = new CommandData()
            {
                CommandText = @"SELECT 2 AS B1, 'apple' AS B2;",
                Bindings = new ICommandBinding[]
                {
                    new ColumnBinding("B1", tuple2[0]),
                    new ColumnBinding("B2", tuple2[1]),
                }
            };

            DatabaseHelper.Default.Execute(command1);
            await DatabaseHelper.Default.ExecuteAsync(command2);

            tuple1[0].Value.ShouldBe(2);
            tuple2[0].Value.ShouldBe(2);

            model1.Value.ShouldBe(2);
            model2.Value.ShouldBe(2);

            tuple1[1].Value.ShouldBe("apple");
            tuple2[1].Value.ShouldBe("apple");

            model1.Value2.ShouldBe("apple");
            model2.Value2.ShouldBe("apple");
        }
        public async Task Test_Execute_WithColumnBindingToIndexer()
        {
            IList<int> model1 = new List<int>() { 0, 0 };
            IList<int> model2 = new List<int>() { 0, 0 };

            IField[] fields1 = DatabaseHelper.Default.Relation(model1, "Item").Column().ToArray();
            IField[] fields2 = DatabaseHelper.Default.Relation(model2, "Item").Column().ToArray();

            CommandData command1 = new CommandData()
            {
                CommandText = @"SELECT 1 AS B1;
                                SELECT 2 AS B2;",
                Bindings = new ICommandBinding[]
                {
                    new ColumnBinding("B1", fields1[0]),
                    new ColumnBinding("B2", fields1[1]),
                }
            };
            CommandData command2 = new CommandData()
            {
                CommandText = @"SELECT 1 AS B1;
                                SELECT 2 AS B2;",
                Bindings = new ICommandBinding[]
                {
                    new ColumnBinding("B1", fields2[0]),
                    new ColumnBinding("B2", fields2[1]),
                }
            };

            DatabaseHelper.Default.Execute(command1);
            await DatabaseHelper.Default.ExecuteAsync(command2);

            fields1.Select(f => (int)f.Value).ShouldBe(new[] { 1, 2 });
            fields2.Select(f => (int)f.Value).ShouldBe(new[] { 1, 2 });

            model1.ShouldBe(new[] { 1, 2 });
            model2.ShouldBe(new[] { 1, 2 });
        }
    }
}
