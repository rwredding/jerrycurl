using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis.Projection;
using Jerrycurl.CodeAnalysis.Razor.Generation;
using Jerrycurl.CodeAnalysis.Razor.Parsing;
using Jerrycurl.CodeAnalysis.Razor.ProjectSystem;
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
            string projectDirectory = args.Options["-p"]?.Value ?? Environment.CurrentDirectory;
            string rootNamespace = args.Options["-ns", "--namespace"]?.Value;
            string sourcePath = Path.GetDirectoryName(typeof(DotNetJerryHost).Assembly.Location);
            string skeletonPath = Path.Combine(sourcePath, "skeleton.jerry");
            string outputDirectory = args.Options["-o", "--output"]?.Value;

            if (!Directory.Exists(projectDirectory))
                throw new RunnerException($"Project directory '{projectDirectory}' does not exist.");

            if (!File.Exists(skeletonPath))
                throw new RunnerException("Skeleton file not found.");

            projectDirectory = PathHelper.MakeAbsolutePath(Environment.CurrentDirectory, projectDirectory);
            outputDirectory = outputDirectory ?? Path.Combine(projectDirectory, "obj", "Jerrycurl");
            outputDirectory = PathHelper.MakeAbsolutePath(projectDirectory, outputDirectory);

            RazorProject project = new RazorProject()
            {
                ProjectDirectory = projectDirectory,
                RootNamespace = rootNamespace,
                Items = new List<RazorProjectItem>(),
            };

            if (args.Options["-f", "--file"] != null)
            {
                string PathResolver(string s) => PathHelper.MakeAbsolutePath(project.ProjectDirectory, s);

                foreach (string file in args.Options["-f", "--file"].Values)
                    foreach (string expandedFile in ToolOptions.ExpandResponseFiles(file, PathResolver))
                        project.AddItem(expandedFile);
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

            GeneratorOptions options = new GeneratorOptions()
            {
                TemplateCode = File.ReadAllText(skeletonPath),
            };

            if (args.Options["--no-clean"] == null && Directory.Exists(outputDirectory))
            {
                DotNetJerryHost.WriteLine("Cleaning...", ConsoleColor.Yellow);

                foreach (string oldFile in Directory.GetFiles(outputDirectory, "*.g.cssql.cs"))
                    File.Delete(oldFile);
            }

            if (args.Options["-i", "--import"] != null)
            {
                foreach (string import in args.Options["-i", "--import"].Values)
                    options.Imports.Add(new RazorFragment() { Text = import });
            }

            if (project.Items.Any())
            {
                Directory.CreateDirectory(outputDirectory);

                RazorParser parser = new RazorParser();
                RazorGenerator generator = new RazorGenerator(options);

                DotNetJerryHost.WriteLine("Parsing...", ConsoleColor.Yellow);
                IList<RazorPage> parserResult = parser.Parse(project).ToList();

                DotNetJerryHost.WriteLine("Transpiling...", ConsoleColor.Yellow);
                foreach (RazorPage razorPage in parserResult)
                {
                    ProjectionResult result = generator.Generate(razorPage.Data);

                    string baseName = Path.GetFileNameWithoutExtension(razorPage.ProjectPath ?? razorPage.Path);
                    string fileName = $"{baseName}.{razorPage.Path.GetHashCode():x2}.g.cssql.cs";
                    string fullName = Path.Combine(outputDirectory, fileName);

                    File.WriteAllText(fullName, result.Content);
                }

                string filesString = project.Items.Count + " " + (project.Items.Count == 1 ? "file" : "files");
                string outputString = PathHelper.MakeRelativeOrAbsolutePath(project.ProjectDirectory, outputDirectory);

                DotNetJerryHost.WriteLine($"Transpiled {filesString} files into '{outputString}'", ConsoleColor.Green);
            }
            else
                DotNetJerryHost.WriteLine($"No files found.", ConsoleColor.Yellow);

            Console.ResetColor();
        }
    }
}
