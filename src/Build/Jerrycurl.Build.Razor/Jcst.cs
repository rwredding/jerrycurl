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

            List<string> tempFiles = new List<string>();

            string tempDir = this.CreateTempDirectory();

            Stopwatch watch = Stopwatch.StartNew();

            foreach (RazorPage razorPage in parser.Parse(project))
            {
                string tempFile = this.GetTempFile(tempDir, razorPage, tempFiles);

                RazorGenerator generator = new RazorGenerator(this.GetGeneratorOptions());
                ProjectionResult result = generator.Generate(razorPage.Data);

                Directory.CreateDirectory(Path.GetDirectoryName(tempFile));
                File.WriteAllText(tempFile, result.Content, Encoding.UTF8);

                this.PrintPageData(razorPage, tempFile);

                tempFiles.Add(tempFile);
            }

            this.Compile = tempFiles.ToArray();

            this.PrintResultData(tempFiles.Count, watch.ElapsedMilliseconds);

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

        private string NormalizePath(string path) => path?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        private string GetTempFile(string tempDir, RazorPage razorFile, IList<string> tempFiles)
        {
            string baseName = Path.GetFileNameWithoutExtension(razorFile.ProjectPath ?? razorFile.Path);
            string fileName = $"{baseName}.{razorFile.Path.GetHashCode():x2}.g.cssql.cs";
            string fullName = Path.Combine(tempDir, this.NormalizePath(fileName));

            int n = 0;

            while (tempFiles.Contains(fullName, StringComparer.OrdinalIgnoreCase))
            {
                fileName = $"{baseName}.{razorFile.Path.GetHashCode():x2}.g{n++}.cssql.cs";

                fullName = Path.Combine(tempDir, this.NormalizePath(fileName));
            }

            return fullName;
        }

        private string CreateTempDirectory()
        {
            string intermediatePath = this.IntermediatePath;

            if (intermediatePath == null)
                intermediatePath = Path.Combine(Environment.CurrentDirectory, "obj", "Jerrycurl");

            intermediatePath = this.NormalizePath(intermediatePath);

            //if (Directory.Exists(intermediatePath))
            //{
            //    try
            //    {
            //        Directory.Delete(intermediatePath, true);
            //    }
            //    catch { }
            //}

            Directory.CreateDirectory(intermediatePath);

            return intermediatePath;
        }

        private GeneratorOptions GetGeneratorOptions()
        {
            string templateCode = null;

            if (File.Exists(this.SkeletonPath))
                templateCode = File.ReadAllText(this.SkeletonPath);

            return new GeneratorOptions()
            {
                TemplateCode = templateCode,
                Imports = new List<RazorFragment>(RazorFacts.GetDefaultNamespaces().Select(ns => new RazorFragment() { Text = ns })),
            };
        }

        private RazorProject CreateRazorProject()
        {
            RazorProject project = new RazorProject()
            {
                RootNamespace = this.RootNamespace,
                Items = this.GetProjectItems().ToList(),
                ProjectDirectory = Environment.CurrentDirectory,
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

            this.PrintMessage("Jerrycurl Build Engine");
            if (version == null)
                this.PrintMessage("\tVersion: <unknown>");
            else if (version.CommitHash != null)
                this.PrintMessage($"\tVersion: {version.PublicVersion} ({version.CommitHash})");
            else
                this.PrintMessage($"\tVersion: {version.PublicVersion}");
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
