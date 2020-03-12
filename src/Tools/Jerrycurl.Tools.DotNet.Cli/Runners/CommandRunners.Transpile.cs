using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis.Projection;
using Jerrycurl.CodeAnalysis.Razor.Generation;
using Jerrycurl.CodeAnalysis.Razor.Parsing;
using Jerrycurl.CommandLine;
using Jerrycurl.IO;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.DotNet.Cli.Scaffolding;
using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal static partial class CommandRunners
    {
        public static void Transpile(RunnerArgs args)
        {
            RazorProject project = new RazorProject()
            {
                RootNamespace = args.Options["-ns", "--namespace"]?.Value ?? "Jerrycurl.Procedures",
                Items = new List<RazorProjectItem>(),
                ProjectDirectory = args.Options["-cwd"]?.Value ?? Environment.CurrentDirectory,
            };

            if (args.Options["-f", "--file"] != null)
            {
                string PathResolver(string s) => PathHelper.MakeAbsolutePath(project.ProjectDirectory, s);

                foreach (string file in args.Options["-f", "--file"].Values)
                    foreach (string file2 in ToolOptions.GetResponseFiles(file, PathResolver))
                        project.AddItem(file2);
            }

            if (args.Options["-d", "--directory"] != null)
            {
                foreach (string dir in args.Options["-d", "--directory"].Values)
                {
                    RazorProject fromDir = RazorProject.FromDirectory(dir);

                    foreach (RazorProjectItem item in fromDir.Items)
                        project.Items.Add(item);
                }
            }

            string sourcePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            string outputDir = args.Options["-o", "--output"]?.Value ?? Path.Combine(Environment.CurrentDirectory, "obj", "Jerrycurl");

            GeneratorOptions options = new GeneratorOptions()
            {
                TemplateCode = File.ReadAllText(Path.Combine(sourcePath, "skeleton.jerry")),
            };

            if (args.Options["--no-clean"] == null && Directory.Exists(outputDir))
            {
                Program.WriteLine("Cleaning...", ConsoleColor.Yellow);

                foreach (string oldFile in Directory.GetFiles(outputDir, "*.g.cssql.cs"))
                    File.Delete(oldFile);
            }

            if (args.Options["-i", "--import"] != null)
            {
                foreach (string import in args.Options["-i", "--import"].Values)
                    options.Imports.Add(new RazorFragment() { Text = import });
            }

            Directory.CreateDirectory(outputDir);

            RazorParser parser = new RazorParser();
            RazorGenerator generator = new RazorGenerator(options);

            Program.WriteLine("Parsing...", ConsoleColor.Yellow);
            IList<RazorPage> parserResult = parser.Parse(project).ToList();

            Program.WriteLine("Transpiling...", ConsoleColor.Yellow);
            foreach (RazorPage razorPage in parserResult)
            {
                ProjectionResult result = generator.Generate(razorPage.Data);

                string baseName = Path.GetFileNameWithoutExtension(razorPage.ProjectPath ?? razorPage.Path);
                string fileName = $"{baseName}.{razorPage.Path.GetHashCode():x2}.g.cssql.cs";
                string fullName = Path.Combine(outputDir, fileName);

                File.WriteAllText(fullName, result.Content);
            }

            string filesString = project.Items.Count + " " + (project.Items.Count == 1 ? "file" : "files");
            string outputString = PathHelper.MakeRelativeOrAbsolutePath(project.ProjectDirectory, outputDir);

            Console.ForegroundColor = ConsoleColor.Green;

            Program.WriteLine($"Transpiled {filesString} files into '{outputString}'");

            Console.ResetColor();
        }
    }
}
