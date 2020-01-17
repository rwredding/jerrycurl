using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jerrycurl.CodeAnalysis;
using Jerrycurl.Text;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Scaffolding
{
    internal class ScaffoldProject
    {
        public IList<ScaffoldFile> Files { get; private set; }
        public DatabaseModel Database { get; private set; }

        public static ScaffoldProject FromModel(DatabaseModel databaseModel, IList<TypeMapping> typeMappings, RunnerArgs args)
        {
            List<ScaffoldFile> files = new List<ScaffoldFile>();

            foreach (var g1 in databaseModel.Tables.GroupBy(t => ResolveFileName(t, databaseModel.DefaultSchema, args)))
            {
                ScaffoldFile newFile = new ScaffoldFile()
                {
                    FileName = g1.Key,
                };

                foreach (TableModel table in g1.Where(t => !t.Ignore))
                {
                    ScaffoldObject newObject = new ScaffoldObject()
                    {
                        Table = table,
                        Namespace = ResolveNamespace(table, databaseModel.DefaultSchema, args),
                        ClassName = ResolveClassName(table),
                    };

                    foreach (ColumnModel column in table.Columns.Where(c => !c.Ignore))
                    {
                        ScaffoldProperty newProperty = new ScaffoldProperty()
                        {
                            Column = column,
                            PropertyName = ResolvePropertyName(newObject, column),
                            TypeName = ResolveTypeName(typeMappings, column),
                        };

                        newObject.Properties.Add(newProperty);
                    }

                    newFile.Objects.Add(newObject);
                }

                if (newFile.Objects.Count > 0)
                    files.Add(newFile);
            }

            return new ScaffoldProject()
            {
                Files = files,
                Database = databaseModel,
            };
        }

        private static string ResolvePropertyName(ScaffoldObject obj, ColumnModel column)
        {
            string propertyName = CSharp.Identifier(column.Name);

            if (propertyName == obj.ClassName)
                return $"{propertyName}0";

            return propertyName;
        }

        private static string ResolveTypeName(IEnumerable<TypeMapping> typeMappings, ColumnModel column)
        {
            TypeMapping mapping = typeMappings.FirstOrDefault(t => String.Equals(t.DbName, column.TypeName, StringComparison.OrdinalIgnoreCase));

            if (mapping != null && mapping.IsValueType && column.IsNullable)
                return mapping.ClrName + "?";
            else if (mapping != null)
                return mapping.ClrName;

            return "object";
        }

        private static string ResolveClassName(TableModel table) => CSharp.Identifier(table.Name);

        private static string ResolveFileName(TableModel table, string defaultSchema, RunnerArgs info)
        {
            if (info.Output == null)
                return "Database.cs";
            else if (info.Output.EndsWith(".cs"))
                return info.Output;
            else if (!string.IsNullOrEmpty(table.Schema) && !table.Schema.Equals(defaultSchema))
                return Path.Combine(info.Output, $"{CSharp.Identifier(table.Schema).ToCapitalCase()}", $"{CSharp.Identifier(table.Name).ToCapitalCase()}.cs");
            else
                return Path.Combine(info.Output, $"{CSharp.Identifier(table.Name).ToCapitalCase()}.cs");
        }

        private static string ResolveNamespace(TableModel table, string defaultSchema, RunnerArgs info)
        {
            Namespace ns;

            if (!string.IsNullOrEmpty(info.Namespace) && !string.IsNullOrEmpty(table.Schema) && !table.Schema.Equals(defaultSchema))
                ns = new Namespace(info.Namespace).Add(table.Schema.ToCapitalCase());
            else if (!string.IsNullOrEmpty(info.Namespace))
                ns = new Namespace(info.Namespace);
            else if (!string.IsNullOrEmpty(table.Schema) && !table.Schema.Equals(defaultSchema))
                ns = new Namespace(table.Schema.ToCapitalCase());
            else
                ns = new Namespace("Database");

            return ns.Definition;
        }
    }
}
