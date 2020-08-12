using Jerrycurl.Relations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Collections;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class Command : IOperation
    {
        public CommandData Data { get; }
        public CommandBuffer Buffer { get; }
        public object Source => this.Data;

        public Command(CommandData commandData, CommandBuffer buffer)
        {
            this.Data = commandData ?? throw new ArgumentNullException(nameof(commandData));
            this.Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void Build(IDbCommand adoCommand)
        {
            foreach (IParameter parameter in this.Data.Parameters ?? Array.Empty<IParameter>())
                this.Buffer.Add(parameter);

            foreach (ICommandBinding binding in this.Data.Bindings ?? Array.Empty<ICommandBinding>())
                this.Buffer.Add(binding);

            adoCommand.CommandText = this.Data.CommandText;

            foreach (IDbDataParameter parameter in this.Buffer.GetParameters(adoCommand))
                adoCommand.Parameters.Add(parameter);
        }
    }
}
