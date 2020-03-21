using System.Collections.Generic;
using System.IO;
using Jerrycurl.CodeAnalysis.Razor.ProjectSystem.Conventions;
using Jerrycurl.IO;

namespace Jerrycurl.CodeAnalysis.Razor.ProjectSystem
{
    public class RazorProject
    {
        public string RootNamespace { get; set; }
        public string ProjectDirectory { get; set; }
        public string IntermediateDirectory { get; set; } = RazorProjectConventions.DefaultIntermediateDirectory;
        public IList<RazorProjectItem> Items { get; set; } = new List<RazorProjectItem>();
        public IEnumerable<IRazorProjectConvention> Conventions { get; set; } = RazorProjectConventions.Default;

        public RazorProjectItem AddItem(string path)
        {
            if (this.Items == null)
                this.Items = new List<RazorProjectItem>();

            RazorProjectItem newItem = RazorProjectItem.Create(path, this.ProjectDirectory);

            this.Items.Add(newItem);

            return newItem;
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
                        ProjectPath = PathHelper.MakeRelativePath(projectDirectory, fullPath),
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
