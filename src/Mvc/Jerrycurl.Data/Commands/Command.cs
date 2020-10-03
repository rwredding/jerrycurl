using System;
using System.Collections.Generic;
using System.Data;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Commands
{
    public class Command : IBatch
    {
        public string CommandText { get; set; }

        public ICollection<IUpdateBinding> Bindings { get; set; } = new List<IUpdateBinding>();
        public ICollection<IParameter> Parameters { get; set; } = new List<IParameter>();

        public void Build(IDbCommand adoCommand)
        {
            CommandBuffer buffer = new CommandBuffer();

            adoCommand.CommandText = this.CommandText;

            foreach (IDbDataParameter parameter in buffer.GetParameters(adoCommand))
                adoCommand.Parameters.Add(parameter);
        }
    }
}
