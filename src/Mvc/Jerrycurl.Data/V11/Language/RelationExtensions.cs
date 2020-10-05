using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Data.V11;
using Jerrycurl.Relations.V11;
using Jerrycurl.Relations.V11.Internal;

namespace Jerrycurl.Data.V11.Language
{
    public static class RelationExtensions
    {
        public static Query ToQuery(this IRelation2 relation, string queryText)
        {
            return new Query()
            {
                QueryText = queryText,
                Parameters = relation.ToParameters()
            };
        }

        public static Command ToCommand(this IRelation2 relation, Func<IList<IParameter>, string> textFactory)
        {
            ParameterStore2 store = new ParameterStore2();

            Command command = new Command()
            {
                Parameters = store,
            };

            using var reader = relation.GetReader();

            while (reader.Read())
            {
                IList<IParameter> parameters = store.Add(reader);

                command.CommandText += textFactory(parameters) + Environment.NewLine;
            }

            return command;
        }

        public static IList<IParameter> ToParameters(this IRelation2 relation)
            => new ParameterStore2().Add(relation);
    }
}
