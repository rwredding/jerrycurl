using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;

namespace Jerrycurl.Mvc
{
    public class ProcBuffer : SqlBuffer, ISqlSerializer<CommandData>, ISqlSerializer<QueryData>
    {

        IEnumerable<CommandData> ISqlSerializer<CommandData>.Serialize(ISqlOptions options)
        {
            foreach (ISqlContent content in this.Read(options))
            {
                yield return new CommandData()
                {
                    CommandText = content.Text,
                    Parameters = content.Parameters.ToList(),
                    Bindings = content.Bindings.ToList(),
                };
            }

        }

        IEnumerable<QueryData> ISqlSerializer<QueryData>.Serialize(ISqlOptions options)
        {
            foreach (ISqlContent content in this.Read(options))
            {
                yield return new QueryData()
                {
                    QueryText = content.Text,
                    Parameters = content.Parameters.ToList(),
                };
            }
        }
    }
}
