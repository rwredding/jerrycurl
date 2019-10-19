using System;

namespace Jerrycurl.CodeAnalysis
{
    internal class TypeName
    {
        public string Name { get; }
        public string FullName => this.Namespace.IsGlobal ? this.Name : this.ToString();
        public Namespace Namespace { get; }

        public TypeName(Type type)
            : this(type?.Name, Namespace.FromType(type))
        {

        }

        public TypeName(string typeName, Namespace ns = null)
        {
            this.Name = Escape(typeName);
            this.Namespace = ns ?? Namespace.Global;
        }

        public static string Escape(string typeName) => CSharp.Identifier(typeName);

        public override string ToString()
        {
            if (this.Namespace.IsGlobal)
                return $"global::{this.Name}";

            return this.Namespace.Definition + "." + this.Name;
        }
    }
}
