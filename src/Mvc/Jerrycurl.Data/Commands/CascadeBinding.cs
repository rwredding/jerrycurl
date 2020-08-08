using System;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands
{
    public class CascadeBinding : ICommandBinding
    {
        public IField Source { get; }
        public IField Target { get; }

        public CascadeBinding(IField source, IField target)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public override string ToString() => this.Source.Identity.Name;
    }
}
