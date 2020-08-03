using System;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands
{
    public class FieldBinding : ICommandBinding
    {
        public IField Source { get; }
        public IField Target { get; }

        public FieldBinding(IField source, IField target)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public override string ToString() => this.Source.Identity.Name;
    }
}
