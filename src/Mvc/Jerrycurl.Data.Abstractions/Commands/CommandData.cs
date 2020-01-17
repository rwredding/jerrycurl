using System.Collections.Generic;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Commands
{
    public class CommandData
    {
        public string CommandText { get; set; }

        public ICollection<ICommandBinding> Bindings { get; set; } = new List<ICommandBinding>();
        public ICollection<IParameter> Parameters { get; set; } = new List<IParameter>();
    }
}
