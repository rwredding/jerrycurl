using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jerrycurl.CodeAnalysis.Razor.Parsing.Directives;
using Jerrycurl.IO;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing.Conventions
{
    public class RazorImportConvention : IRazorProjectConvention
    {
        public void Apply(RazorProject project, IList<RazorPage> result)
        {
            List<RazorPage> importPages = new List<RazorPage>();

            foreach (RazorPage page in result.Where(this.IsImport))
                importPages.Add(page);

            importPages = importPages.OrderBy(p => p.ProjectPath.Length).ToList();

            foreach (RazorPage importPage in importPages)
                result.Remove(importPage);

            foreach (RazorPage resultPage in result)
                this.ApplyFileImports(resultPage, importPages);
        }

        private void ApplyFileImports(RazorPage resultPage, List<RazorPage> importPages)
        {
            List<RazorPage> validImports = importPages.Where(i => this.IsImportFor(i, resultPage)).ToList();

            List<RazorFragment> newImports = new List<RazorFragment>();
            List<InjectDirective> newProjections = new List<InjectDirective>();
            List<InjectDirective> newInjections = new List<InjectDirective>();

            foreach (RazorPage importPage in validImports)
            {
                newImports.AddRange(importPage.Data.Imports);
                newProjections.AddRange(importPage.Data.Projections);
                newInjections.AddRange(importPage.Data.Injections);
            }

            resultPage.Data.Imports = newImports.Concat(resultPage.Data.Imports).ToList();
            resultPage.Data.Projections = newProjections.Concat(resultPage.Data.Projections).ToList();
            resultPage.Data.Injections = newInjections.Concat(resultPage.Data.Injections).ToList();

            RazorPage templateFile = validImports.Where(f => f.Data.Template != null).LastOrDefault();

            if (resultPage.Data.Template == null && templateFile != null)
                resultPage.Data.Template = templateFile.Data.Template;
        }

        private bool IsImportFor(RazorPage importPage, RazorPage resultPage)
        {
            if (resultPage.ProjectPath == null)
                return false;

            string importDir = Path.GetDirectoryName(importPage.ProjectPath);

            return PathHelper.IsRelativeTo(resultPage.ProjectPath, importDir);
        }

        private bool IsImport(RazorPage page)
        {
            if (page.ProjectPath == null)
                return false;

            return Path.GetFileName(page.ProjectPath).Equals("_imports.cssql", StringComparison.OrdinalIgnoreCase);
        }
    }
}
