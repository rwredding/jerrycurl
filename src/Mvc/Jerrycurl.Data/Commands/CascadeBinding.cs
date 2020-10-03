using System;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands
{
    public class CascadeBinding : IUpdateBinding
    {
        public IField Source { get; }
        public IField Target { get; }

        public CascadeBinding(IField source, IField target)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public override string ToString() => $"ParameterBinding: {this.Source.Identity.Name} -> {this.Target.Identity.Name}";
    }
}
