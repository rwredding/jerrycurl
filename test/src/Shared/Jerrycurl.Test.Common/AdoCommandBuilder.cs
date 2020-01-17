using System.Data;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Test
{
    public class AdoCommandBuilder : IOperation
    {
        private readonly string commandText;

        public object Source => null;

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
