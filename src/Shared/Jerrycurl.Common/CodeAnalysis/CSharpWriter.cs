using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Jerrycurl.CodeAnalysis
{
    internal class CSharpWriter
    {
        public CSharpFlags Flags { get; }
		protected TextWriter TextWriter { get; }

		private int indentation = 0;
        private readonly List<AttributeModel> attributeBuffer = new List<AttributeModel>();

		public CSharpWriter(TextWriter textWriter, CSharpFlags flags = CSharpFlags.AutoIndent)
		{
			this.TextWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
            this.Flags = flags;
		}

        public async Task WriteImportAsync(string definition)
        {
            await this.WriteIndentAsync();
            await this.TextWriter.WriteLineAsync($"using {definition.TrimEnd().TrimEnd(';')};");
        }

        public async Task WriteIndentAsync() => await this.TextWriter.WriteAsync(new string('\t', this.indentation));

        public async Task WriteCommentAsync(string commentText)
        {
            await this.TextWriter.WriteLineAsync("//" + commentText);
        }
        public async Task WriteLineAsync()
        {
            await this.WriteIndentAsync();
            await this.TextWriter.WriteLineAsync();
        }

        public async Task WriteErrorDirectiveAsync(string message) => await this.TextWriter.WriteLineAsync($"#error {message}");
        public async Task WriteWarningDirectiveAsync(string message) => await this.TextWriter.WriteLineAsync($"#warning {message}");

        public void AddAttribute(string typeName, params object[] arguments) => this.AddAttribute(typeName, (IEnumerable<object>)arguments);
        public void AddAttribute(string typeName, IEnumerable<object> arguments) => this.attributeBuffer.Add(new AttributeModel(typeName, arguments));

        public async Task WriteAttributesAsync()
        {
            if (this.attributeBuffer.Any())
            {
                await this.WriteIndentAsync();
                await this.TextWriter.WriteAsync("[");
                await this.WriteAttributeContentAsync(this.attributeBuffer.First());

                foreach (AttributeModel attribute in this.attributeBuffer.Skip(1))
                {
                    await this.TextWriter.WriteAsync(", ");
                    await this.WriteAttributeContentAsync(attribute);
                }

                await this.TextWriter.WriteAsync("]");
                await this.TextWriter.WriteLineAsync();

                this.attributeBuffer.Clear();
            }
        }

        private async Task WriteAttributeContentAsync(AttributeModel attribute)
        {
            await this.TextWriter.WriteAsync(attribute.TypeName);

            if (attribute.Arguments.Any())
            {
                await this.TextWriter.WriteAsync("(");
                await this.TextWriter.WriteAsync(string.Join(", ", attribute.Arguments.Select(this.FormatAttributeLiteral)));
                await this.TextWriter.WriteAsync(")");
            }
        }

        public async Task WritePropertyAsync(string type, string name, string[] modifiers = null)
        {
            await this.WriteIndentAsync();

            if (modifiers?.Length > 0)
            {
                await this.TextWriter.WriteAsync(string.Join(" ", modifiers));
                await this.TextWriter.WriteAsync(" ");
            }

            await this.TextWriter.WriteAsync(type);
            await this.TextWriter.WriteAsync(" ");
            await this.TextWriter.WriteAsync(name);
            await this.TextWriter.WriteLineAsync(" { get; set; }");
        }

        public async Task WriteObjectStartAsync(string type, string name, string[] modifiers = null, string[] baseTypes = null)
        {
            await this.WriteIndentAsync();

            if (modifiers?.Length > 0)
            {
                await this.TextWriter.WriteAsync(string.Join(" ", modifiers));
                await this.TextWriter.WriteAsync(" ");
            }

            await this.TextWriter.WriteAsync(type);
            await this.TextWriter.WriteAsync(" ");
            await this.TextWriter.WriteAsync(CSharp.Identifier(name));

            if (baseTypes?.Length > 0)
            {
                await this.TextWriter.WriteAsync(" : ");
                await this.TextWriter.WriteAsync(string.Join(", ", baseTypes));
            }

            await this.WriteLineAsync();
            await this.WriteIndentAsync();
            await this.TextWriter.WriteLineAsync("{");

            if (this.HasFlag(CSharpFlags.AutoIndent))
                this.Indent();
        }

        public async Task WriteObjectEndAsync()
        {
            if (this.HasFlag(CSharpFlags.AutoIndent))
                this.Undent();

            await this.WriteIndentAsync();
            await this.TextWriter.WriteLineAsync("}");
        }

        public async Task WriteNamespaceEndAsync()
        {
            if (this.HasFlag(CSharpFlags.AutoIndent))
                this.Undent();

            await this.WriteIndentAsync();
            await this.TextWriter.WriteLineAsync("}");
        }

        public void Indent()
        {
            this.indentation++;
        }

        public void Undent()
        {
            this.indentation = Math.Max(this.indentation - 1, 0);
        }

        public async Task WriteNamespaceStartAsync(string ns)
        {
            await this.WriteIndentAsync();
            await this.TextWriter.WriteLineAsync($"namespace {ns}");
            await this.WriteIndentAsync();
            await this.TextWriter.WriteLineAsync("{");

            if (this.HasFlag(CSharpFlags.AutoIndent))
                this.Indent();
        }

        private bool HasFlag(CSharpFlags flags) => this.Flags.HasFlag(flags);

        private string FormatAttributeLiteral(object value)
        {
            if (value is string s && (!s.StartsWith("typeof(") || !s.EndsWith(")")))
                return CSharp.Literal(s);

            return value?.ToString() ?? "null";
        }

        private class AttributeModel
        {
            public string TypeName { get; }
            public IEnumerable<object> Arguments { get; }

            public AttributeModel(string typeName, IEnumerable<object> arguments)
            {
                this.TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
                this.Arguments = arguments ?? Array.Empty<object>();
            }
        }
    }
}
