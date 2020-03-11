using System.Collections.Generic;
using System.IO;
using Jerrycurl.IO;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing
{
    public class RazorProject
    {
        public string RootNamespace { get; set; }
        public string ProjectDirectory { get; set; }
        public IList<RazorProjectItem> Items { get; set; } = new List<RazorProjectItem>();

        public void AddItem(string path)
        {
            if (this.Items == null)
                this.Items = new List<RazorProjectItem>();

            this.Items.Add(RazorProjectItem.Create(path, this.ProjectDirectory));
        }

        public static RazorProject FromDirectory(string projectDirectory, string rootNamespace = null, string filePattern = "*.cssql")
        {
            DirectoryInfo sourceInfo = new DirectoryInfo(projectDirectory);

            if (sourceInfo.Exists)
            {
                List<RazorProjectItem> projectItems = new List<RazorProjectItem>();

                foreach (string fullPath in Directory.GetFiles(sourceInfo.FullName, filePattern, SearchOption.AllDirectories))
                {
                    RazorProjectItem newItem = new RazorProjectItem()
                    {
                        FullPath = fullPath,
                        ProjectPath = PathHelper.MakeRelativePath(fullPath, projectDirectory),
                    };

                    projectItems.Add(newItem);
                }

                return new RazorProject()
                {
                    RootNamespace = rootNamespace,
                    ProjectDirectory = projectDirectory,
                    Items = projectItems,
                };
            }

            throw new DirectoryNotFoundException($"Source directory '{projectDirectory}' not found.");
        }
    }
}
