using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis;
using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Scaffolding
{
    internal static class ScaffoldWriter
    {
        public static async Task WriteAsync(ScaffoldProject project)
        {
            foreach (ScaffoldFile file in project.Files)
            {
                using (StreamWriter stream = GetStream(file.FileName))
                {
                    CSharpWriter csharp = new CSharpWriter(stream);

                    await csharp.WriteImportAsync("global::System");
                    await csharp.WriteImportAsync("global::Jerrycurl.Data.Metadata.Annotations");
                    await csharp.WriteLineAsync();

                    foreach (var g in file.Objects.GroupBy(t => t.Namespace).OrderBy(g => g.Key))
                    {
                        string ns = g.Key;

                        if (!string.IsNullOrEmpty(ns))
                            await csharp.WriteNamespaceStartAsync(ns);

                        string[] modifiers = new[] { "public" };

                        foreach (ScaffoldObject obj in g.OrderBy(t => t.ClassName))
                        {
                            if (!string.IsNullOrEmpty(obj.Table.Schema))
                                csharp.AddAttribute("Table", obj.Table.Schema, obj.Table.Name);
                            else
                                csharp.AddAttribute("Table", obj.Table.Name);

                            await csharp.WriteAttributesAsync();
                            await csharp.WriteObjectStartAsync("class", obj.ClassName, modifiers);

                            foreach (ScaffoldProperty property in obj.Properties)
                            {
                                if (property.PropertyName != property.Column.Name)
                                    csharp.AddAttribute("Column", property.Column.Name);

                                if (property.Column.IsIdentity)
                                    csharp.AddAttribute("Id");

                                foreach (KeyModel key in property.Column.Keys)
                                    csharp.AddAttribute("Key", key.Name, key.Index);

                                foreach (ReferenceModel @ref in property.Column.References)
                                    csharp.AddAttribute("Ref", @ref.KeyName, @ref.KeyIndex, @ref.Name);

                                await csharp.WriteAttributesAsync();
                                await csharp.WritePropertyAsync(property.TypeName, property.PropertyName, modifiers);
                            }

                            await csharp.WriteObjectEndAsync();
                            await csharp.WriteLineAsync();
                        }

                        if (!string.IsNullOrEmpty(ns))
                            await csharp.WriteNamespaceEndAsync();
                    }
                }
            }
        }

        private static StreamWriter GetStream(string fileName)
        {
            string directoryName = Path.GetDirectoryName(fileName);

            if (directoryName != "" && !Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            return new StreamWriter(fileName);
        }
    }
}
