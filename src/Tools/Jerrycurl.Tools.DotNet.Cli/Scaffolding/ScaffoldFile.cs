using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Scaffolding
{
    internal class ScaffoldFile
    {
        public string FileName { get; set; }
        public IList<ScaffoldObject> Objects { get; set; } = new List<ScaffoldObject>();
    }
}
