using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal
{
    internal class FuncArguments
    {
        public ParameterExpression Source { get; } = Expression.Parameter(typeof(IField), "source");
        public ParameterExpression Model { get; } = Expression.Parameter(typeof(IField), "model");
        public ParameterExpression Fields { get; } = Expression.Parameter(typeof(IField[]), "fields");
        public ParameterExpression Attributes { get; } = Expression.Parameter(typeof(MetadataIdentity[]), "attributes");
        public ParameterExpression Binders { get; } = Expression.Parameter(typeof(Delegate[]), "binders");
        public ParameterExpression Enumerators { get; } = Expression.Parameter(typeof(IEnumerator[]), "enumerators");
        public ParameterExpression Notation { get; } = Expression.Parameter(typeof(IMetadataNotation), "notation");
        public ParameterExpression[] ItemVars { get; }

        public FuncArguments(int itemCount)
        {
            this.ItemVars = new ParameterExpression[itemCount];
        }

        public IEnumerable<ParameterExpression> GetParameters()
        {
            yield return this.Model;
            yield return this.Source;
            yield return this.Attributes;
            yield return this.Binders;
            yield return this.Enumerators;
            yield return this.Fields;
            yield return this.Notation;
        }

        public IEnumerable<ParameterExpression> GetVariables()
        {
            return this.ItemVars.Where(v => v != null);
        }
    }
}
