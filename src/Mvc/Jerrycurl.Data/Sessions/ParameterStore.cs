using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Jerrycurl.Data.Commands;
using Jerrycurl.Relations;
using Jerrycurl.Relations.V11.Internal;

namespace Jerrycurl.Data.Sessions
{
    public class ParameterStore : Collection<IParameter>
    {
        private readonly Dictionary<IField, IParameter> innerMap = new Dictionary<IField, IParameter>();

        public char? Prefix { get; }

        public ParameterStore(char? prefix = null)
        {
            this.Prefix = prefix;
        }

        public IParameter Add(IField field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (!this.innerMap.TryGetValue(field, out IParameter param))
            {
                string paramName = $"{this.Prefix}P{this.innerMap.Count}";

                this.innerMap.Add(field, param = new Parameter(paramName, field));
                this.Add(param);
            }

            return param;
        }

        public IReadOnlyList<IParameter> Add(ITuple tuple) => tuple.Select(this.Add).ToList();
    }
}
