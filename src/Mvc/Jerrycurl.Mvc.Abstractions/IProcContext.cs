using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc
{
    public interface IProcContext
    {
        IProcLocator Locator { get; }
        IDomainOptions Domain { get; }
        IProcRenderer Renderer { get; }
        IProcLookup Lookup { get; }
        IPageExecutionContext Executing { get; }
    }
}
