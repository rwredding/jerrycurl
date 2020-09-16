using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;

namespace Jerrycurl.Mvc
{
    public class ProcBuffer : SqlBuffer, ISqlSerializer<Command>, ISqlSerializer<Query>
    {
        IEnumerable<Command> ISqlSerializer<Command>.Serialize(ISqlOptions options)
        {
            foreach (ISqlContent content in this.Read(options))
            {
                yield return new Command()
                {
                    CommandText = content.Text,
                    Parameters = content.Parameters.ToList(),
                    Bindings = content.Bindings.ToList(),
                };
            }

        }

        IEnumerable<Query> ISqlSerializer<Query>.Serialize(ISqlOptions options)
        {
            foreach (ISqlContent content in this.Read(options))
            {
                yield return new Query()
                {
                    QueryText = content.Text,
                    Parameters = content.Parameters.ToList(),
                };
            }
        }
    }
}
