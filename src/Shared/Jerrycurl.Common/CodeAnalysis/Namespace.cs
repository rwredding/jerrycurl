using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jerrycurl.CodeAnalysis
{
    internal sealed class Namespace : IEquatable<Namespace>
    {
        private readonly string[] parts;

        public string Definition { get; }
        public int Level => this.parts.Length;
        public bool IsGlobal => (this.Level == 0);

        public static Namespace Global { get; } = new Namespace("");

        public static Namespace FromType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return new Namespace(type.Namespace);
        }

        public static bool IsValid(string definition)
        {
            if (definition == null)
                return false;

            return new Namespace(definition).Definition.Equals(definition);
        }

        public static Namespace FromPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            char[] c = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

            IEnumerable<string> parts = path.Split(c, StringSplitOptions.RemoveEmptyEntries);

            Namespace ns = Global;

            foreach (string part in parts)
            {
                if (part == "..")
                    ns = ns.Up();
                else if (part != ".")
                    ns = ns.Add(CSharp.Identifier(part));
            }

            return ns;
        }

        public static string Escape(string definition) => new Namespace(definition).Definition;

        public string ToPath() => string.Join(Path.DirectorySeparatorChar.ToString(), this.parts);
        public TypeName ToTypeName(string typeName) => new TypeName(typeName, this);

        public TypeName AsTypeName()
        {
            if (this.IsGlobal)
                throw new InvalidOperationException("No type name available in global namespace.");

            return new TypeName(this.parts.Last(), this.Up());
        }

        public Namespace(string definition)
        {
            this.Definition = definition ?? "";
            this.parts = this.Definition.Split(new[] { '.' }).Select(this.EscapePart).Where(p => p.Length > 0).ToArray();
        }

        private Namespace(string[] parts)
        {
            this.parts = parts;
            this.Definition = string.Join(".", parts);
        }

        private string EscapePart(string part) => string.IsNullOrWhiteSpace(part) ? "" : CSharp.Identifier(part.Trim());

        public Namespace Add(string definition) => this.Add(new Namespace(definition));
        public Namespace Add(Namespace ns)
        {
            if (ns == null)
                throw new ArgumentNullException(nameof(ns));

            return new Namespace(this.parts.Concat(ns.parts).ToArray());
        }

        public IEnumerable<Namespace> Traverse()
        {
            Namespace ns = this;

            yield return ns;

            while (ns.Level > 0)
                yield return (ns = ns.Up());
        }

        public bool Contains(Type type) => FromType(type).Equals(this);

        public Namespace Up()
        {
            if (this.Level <= 1)
                return Global;

            return new Namespace(this.parts.Take(this.parts.Length - 1).ToArray());
        }

        public bool IsParentOf(Namespace ns)
        {
            if (ns == null)
                throw new ArgumentNullException(nameof(ns));

            if (this.Level >= ns.Level)
                return false;

            return this.parts.SequenceEqual(ns.parts.Take(this.Level));
        }

        public Namespace Sanitize(Func<string, string> sanitizer)
        {
            if (sanitizer == null)
                throw new ArgumentNullException(nameof(sanitizer));

            string definition = string.Join(".", this.parts.Select(sanitizer).Where(s => s?.Length > 0));

            return new Namespace(definition);
        }

        public bool IsChildOf(Namespace ns) => ns?.IsParentOf(this) ?? throw new ArgumentNullException(nameof(ns));

        public bool Equals(Namespace other) => StringComparer.Ordinal.Equals(this.Definition, other?.Definition);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(this.Definition);
        public override bool Equals(object obj) => (obj is Namespace other && this.Equals(other));

        public override string ToString() => this.Definition ?? "<global>";
    }
}
