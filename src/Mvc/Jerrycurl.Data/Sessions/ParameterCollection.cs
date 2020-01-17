using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Sessions
{
    public class ParameterCollection : Collection<IParameter>
    {
        private readonly Dictionary<FieldIdentity, IParameter> innerMap = new Dictionary<FieldIdentity, IParameter>();

        public char? Prefix { get; }

        public ParameterCollection(char? prefix = null)
        {
            this.Prefix = prefix;
        }

        public IParameter Add(IField field)
        {
            if (!this.innerMap.TryGetValue(field.Identity, out IParameter param))
            {
                string paramName = $"{this.Prefix}P{this.innerMap.Count}";

                this.innerMap.Add(field.Identity, param = new Parameter(paramName, field));
                this.Add(param);
            }

            return param;
        }

        public IReadOnlyList<IParameter> Add(ITuple tuple) => tuple.Select(this.Add).ToList();
    }
}
