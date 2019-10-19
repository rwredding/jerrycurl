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
    public interface IPageExecutionContext
    {
        PageDescriptor Page { get; }
        ISqlBuffer Buffer { get; }
        BodyFactory Body { get; }
    }
}
