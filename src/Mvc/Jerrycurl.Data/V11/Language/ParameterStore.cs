using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations;
using Jerrycurl.Relations.V11;
using Jerrycurl.Relations.V11.Internal;

namespace Jerrycurl.Data.V11.Language
{
    public class ParameterStore2 : Collection<IParameter>
    {
        private readonly Dictionary<IField2, IParameter> innerMap = new Dictionary<IField2, IParameter>();

        public char? Prefix { get; }

        public ParameterStore2(char? prefix = null)
        {
            this.Prefix = prefix;
        }

        public IParameter Add(IField2 field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (!this.innerMap.TryGetValue(field, out IParameter param))
            {
                string paramName = $"{this.Prefix}P{this.innerMap.Count}";

                this.innerMap.Add(field, param = new Parameter(paramName, null)); // should be field
                this.Add(param);
            }

            return param;
        }

        public IList<IParameter> Add(ITuple2 tuple)
            => tuple?.Select(this.Add).ToList() ?? throw new ArgumentNullException(nameof(tuple));

        public IList<IParameter> Add(IRelation2 relation)
        {
            if (relation == null)
                throw new ArgumentNullException(nameof(relation));

            using IRelationReader reader = relation.GetReader();

            List<IParameter> parameters = new List<IParameter>();

            while (reader.Read())
                parameters.AddRange(this.Add(reader));

            return parameters;
        }
    }
}
