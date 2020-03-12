using System.Collections.Generic;
using System.IO;
using Jerrycurl.CodeAnalysis.Razor.Parsing;

namespace Jerrycurl.CodeAnalysis.Razor.ProjectSystem.Conventions
{
    public class RazorNamingConvention : IRazorProjectConvention
    {
        public void Apply(RazorProject project, IList<RazorPage> result)
        {
            foreach (RazorPage razorFile in result)
                this.ApplyConventions(project, razorFile);
        }

        private void ApplyConventions(RazorProject project, RazorPage razorPage)
        {
            if (razorPage.Data.Class == null)
                razorPage.Data.Class = this.GetClassDirectiveFromFileName(razorPage);

            if (razorPage.ProjectPath != null && razorPage.Data.Namespace == null)
                razorPage.Data.Namespace = this.GetNamespaceDirectiveFromFileName(project, razorPage);
        }

        private RazorFragment GetClassDirectiveFromFileName(RazorPage razorPage)
        {
            return new RazorFragment()
            {
                Text = CSharp.Identifier(Path.GetFileName(razorPage.ProjectPath ?? razorPage.Path)),
            };
        }

        private RazorFragment GetNamespaceDirectiveFromFileName(RazorProject project, RazorPage razorPage)
        {
            Namespace ns = Namespace.FromPath(Path.GetDirectoryName(razorPage.ProjectPath));

            if (!string.IsNullOrEmpty(project.RootNamespace))
                ns = new Namespace(project.RootNamespace).Add(ns);

            return new RazorFragment()
            {
                Text = ns.ToString(),
            };
        }
    }
}
