using System.Data;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Test
{
    public class SqlOperation : IOperation
    {
        private readonly string commandText;

        public object Source => null;

        public SqlOperation(string commandText)
        {
            this.commandText = commandText;
        }       

        public void Build(IDbCommand adoCommand)
        {
            adoCommand.CommandText = this.commandText;
        }
    }
}
