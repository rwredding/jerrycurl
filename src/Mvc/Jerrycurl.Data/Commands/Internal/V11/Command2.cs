using Jerrycurl.Relations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Collections;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class Command2 : IOperation
    {
        public CommandData Data { get; }
        public CommandBuffer Buffer { get; }
        public object Source => this.Data;

        public Command2(CommandData commandData, CommandBuffer buffer)
        {
            this.Data = commandData ?? throw new ArgumentNullException(nameof(commandData));
            this.Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void Build(IDbCommand adoCommand)
        {
            foreach (IParameter parameter in this.Data.Parameters)
                this.Buffer.Add(parameter);

            foreach (var g in this.Data.Bindings.GroupBy(b => b.Target).Select(g => g.ToArray()))
            {
                ParameterBinding paramBinding = g.FirstOfType<ParameterBinding>();
                ColumnBinding columnBinding = g.FirstOfType<ColumnBinding>();
                CascadeBinding cascadeBinding = g.FirstOfType<CascadeBinding>();

                if (columnBinding != null)
                    this.Buffer.Add(columnBinding);
                else if (paramBinding != null)
                    this.Buffer.Add(paramBinding);
                /*else if (cascadeBinding != null)
                    this.Buffer.Add(cascadeBinding);*/
                else
                    throw new CommandException("ICommandBinding must be a ColumnBinding, ParameterBinding or CascadeBinding instance.");
            }

            adoCommand.CommandText = this.Data.CommandText;

            foreach (IDbDataParameter parameter in this.Buffer.GetParameters(adoCommand))
                adoCommand.Parameters.Add(parameter);
        }
    }
}
