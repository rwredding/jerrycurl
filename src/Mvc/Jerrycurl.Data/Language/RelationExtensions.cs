using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations.Internal.V11;

namespace Jerrycurl.Data.Language
{
    public static class RelationExtensions
    {
        public static Query ToQuery(this IRelation3 relation, string queryText)
        {
            return new Query()
            {
                QueryText = queryText,
                Parameters = relation.ToParameters()
            };
        }

        public static Command ToCommand(this IRelation3 relation, Func<IReadOnlyList<IParameter>, string> commandText)
        {
            ParameterStore store = new ParameterStore();
            Command command = new Command()
            {
                Parameters = store,
            };

            using var reader = relation.GetReader();

            while (reader.Read())
            {
                IReadOnlyList<IParameter> parameters = store.Add(reader);

                command.CommandText += sqlFactory(parameters) + Environment.NewLine;
            }

            return command;
        }

        public static IList<IParameter> ToParameters(this IRelation3 relation)
        {
            ParameterStore store = new ParameterStore();

            using var reader = relation.GetReader();

            while (reader.Read())
            {
                IReadOnlyList<IParameter> parameters = store.Add(reader);

                command.CommandText += sqlFactory(parameters) + Environment.NewLine;
            }

            return command;
        }
    }
}
