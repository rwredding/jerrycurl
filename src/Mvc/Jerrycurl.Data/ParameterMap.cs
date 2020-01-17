using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Jerrycurl.Relations;

namespace Jerrycurl.Data
{
    public class ParameterMap : Collection<IParameter>
    {
        private readonly Dictionary<FieldIdentity, IParameter> innerMap = new Dictionary<FieldIdentity, IParameter>();

        public char? Prefix { get; }

        public ParameterMap(char? prefix = null)
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

        public IEnumerable<IParameter> Add(ITuple tuple)
        {
            foreach (IField field in tuple)
                yield return this.Add(field);
        }
    }
}
