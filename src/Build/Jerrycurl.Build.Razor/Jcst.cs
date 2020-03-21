using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Jerrycurl.CodeAnalysis;
using Jerrycurl.CodeAnalysis.Projection;
using Jerrycurl.CodeAnalysis.Razor.Generation;
using Jerrycurl.CodeAnalysis.Razor.Parsing;
using Jerrycurl.CodeAnalysis.Razor.ProjectSystem;
using Jerrycurl.Facts;
using Jerrycurl.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Jerrycurl.Build.Razor
{
    public class Jcst : Task
    {
        [Required]
        public ITaskItem[] Sources { get; set; }

        [Output]
        public string[] Compile { get; set; }

        public string SkeletonPath { get; set; }
        public string RootNamespace { get; set; }
        public string IntermediatePath { get; set; }
        public string ProjectName { get; set; }
        public bool Verbose { get; set; }

        public override bool Execute()
        {
            RazorProject project = this.CreateRazorProject();
            RazorParser parser = new RazorParser();

            this.PrintProjectData(project);

            List<string> filesToCompile = new List<string>();

            Stopwatch watch = Stopwatch.StartNew();

            foreach (RazorPage razorPage in parser.Parse(project))
            {
                RazorGenerator generator = new RazorGenerator(this.CreateGeneratorOptions());
                ProjectionResult result = generator.Generate(razorPage.Data);

                Directory.CreateDirectory(Path.GetDirectoryName(razorPage.IntermediatePath));
                File.WriteAllText(razorPage.IntermediatePath, result.Content, Encoding.UTF8);

                this.PrintPageData(razorPage, razorPage.IntermediatePath);

                filesToCompile.Add(razorPage.IntermediatePath);
            }

            this.Compile = filesToCompile.ToArray();

            this.PrintResultData(filesToCompile.Count, watch.ElapsedMilliseconds);

            return true;
        }

        private string GetFullyQualifiedPageName(RazorPage razorPage)
        {
            string escapedNs = Namespace.Escape(razorPage.Data.Namespace?.Text);
            string escapedClass = string.IsNullOrWhiteSpace(razorPage.Data.Class?.Text) ? "" : CSharp.Identifier(razorPage.Data.Class.Text.Trim());

            if (escapedClass.Length == 0 && escapedNs.Length == 0)
                return "<invalid>";
            else if (escapedNs.Length == 0)
                return $"global::{escapedClass}";
            else if (escapedClass.Length == 0)
                return $"{escapedNs}.<invalid>";
            else
                return $"{escapedNs}.{escapedClass}";
        }

        private GeneratorOptions CreateGeneratorOptions()
        {
            string templateCode = null;

            if (File.Exists(this.SkeletonPath))
                templateCode = File.ReadAllText(this.SkeletonPath);

            return new GeneratorOptions()
            {
                TemplateCode = templateCode,
                Imports = RazorFacts.DefaultNamespaces.Select(ns => new RazorFragment() { Text = ns }).ToList(),
            };
        }

        private RazorProject CreateRazorProject()
        {
            RazorProject project = new RazorProject()
            {
                RootNamespace = this.RootNamespace,
                Items = this.GetProjectItems().ToList(),
                ProjectDirectory = Environment.CurrentDirectory,
                IntermediateDirectory = this.IntermediatePath ?? RazorProjectConventions.DefaultIntermediateDirectory,
            };

            if (string.IsNullOrWhiteSpace(project.RootNamespace))
                project.RootNamespace = this.ProjectName;

            if (string.IsNullOrWhiteSpace(project.RootNamespace))
                project.RootNamespace = null;

            return project;
        }

        private IEnumerable<RazorProjectItem> GetProjectItems()
        {
            foreach (ITaskItem taskItem in this.Sources)
            {
                string fullPath = taskItem.GetMetadata("FullPath");
                string specPath = taskItem.ItemSpec;
                string linkPath = taskItem.GetMetadata("Link");

                yield return new RazorProjectItem()
                {
                    ProjectPath = string.IsNullOrEmpty(linkPath) ? specPath : linkPath,
                    FullPath = fullPath,
                };
            }
        }

        #region " Logging "
        private void PrintMessage(string message)
        {
            MessageImportance importance = this.Verbose ? MessageImportance.High : MessageImportance.Normal;

            this.Log.LogMessage(importance, message);
        }

        private void PrintProjectData(RazorProject project)
        {
            NuGetVersion version = typeof(Jcst).Assembly.GetNuGetPackageVersion();

            if (version == null)
                this.PrintMessage($"Jerrycurl Build Engine");
            else
                this.PrintMessage($"Jerrycurl Build Engine v{version.PublicVersion} ({version.CommitHash})");
            this.PrintMessage($"\tProjectName: {this.ProjectName}");
            this.PrintMessage($"\tProjectDirectory: {project.ProjectDirectory}");
            this.PrintMessage($"\tRootNamespace: {this.RootNamespace}");
            this.PrintMessage($"\tSkeletonPath: {this.SkeletonPath}");
            this.PrintMessage($"\tIntermediatePath: {this.IntermediatePath}");

            if (project.Items.Any())
                this.PrintMessage("\tTranspiling:");
        }

        private void PrintPageData(RazorPage razorPage, string tempFile)
        {
            this.PrintMessage($"\t\t{razorPage.ProjectPath ?? razorPage.Path}");
            this.PrintMessage($"\t\t\t-> {tempFile}");
            this.PrintMessage($"\t\t\t\t-> {this.GetFullyQualifiedPageName(razorPage)}");
        }

        private void PrintResultData(int fileCount, long elapsedMs)
        {
            this.PrintMessage($"\tTranspiled: {fileCount} files in {elapsedMs:0} ms.");
            this.PrintMessage("");
        }
        #endregion
    }
}
