using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Jerrycurl.Data;

namespace Jerrycurl.Test
{
    public class AdoCommandBuilder : IAdoCommandBuilder
    {
        private readonly string commandText;

        public AdoCommandBuilder(string commandText)
        {
            this.commandText = commandText;
        }

        public void Build(IDbCommand adoCommand)
        {
            adoCommand.CommandText = this.commandText;
        }
    }
}
