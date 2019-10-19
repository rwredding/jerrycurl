using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Data.Filters;

namespace Jerrycurl.Data.Commands
{
    public class CommandData
    {
        public string CommandText { get; set; }

        public ICollection<ICommandBinding> Bindings { get; set; } = new List<ICommandBinding>();
        public ICollection<IParameter> Parameters { get; set; } = new List<IParameter>();
    }
}
