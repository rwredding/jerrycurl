using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;
using Jerrycurl.CodeAnalysis.Projection.Internal;

namespace Jerrycurl.CodeAnalysis.Projection
{
    public class Projector
    {
        private readonly Dictionary<string, List<(string, SourceSpan?)>> buffers = new Dictionary<string, List<(string text, SourceSpan? span)>>();

        private Token[] tokens;
        private string[] chunks;
        private string currentType;

        public bool InertMode { get; set; }

        public Projector(string template)
            : this(new StringSource(template))
        {

        }

        public Projector(ISource template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            this.ParseTemplate(template);
        }

        public bool IsEmpty(string type) => !this.buffers.ContainsKey("$" + type + "$");

        private void ParseTemplate(ISource template)
        {
            Lexer lexer = new Lexer(template);

            lexer.Yield(new Chunks());

            this.tokens = lexer.Tokenize().ToArray();
            this.chunks = this.tokens.Select(t => template.GetText(t.Span)).ToArray();
        }

        public Projector WriteLine() => this.Write(Environment.NewLine);

        public Projector WriteLine(string text) => this.Write(text).WriteLine();
        public Projector Write(string text) => this.WriteInternal(text, null);
        public Projector Write(string text, SourceSpan span) => this.WriteInternal(text, span);

        private Projector WriteInternal(string text, SourceSpan? span)
        {
            if (this.currentType == null)
                throw new InvalidOperationException("No type is selected.");

            if (!this.buffers.TryGetValue(this.currentType, out var list))
                this.buffers[this.currentType] = list = new List<(string text, SourceSpan? span)>();

            list.Add((text, this.InertMode ? null : span));

            return this;
        }

        public Projector Open(string type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            this.currentType = "$" + type + "$";

            return this;
        }

		public ProjectionResult Generate()
		{
			StringBuilder builder = new StringBuilder();
			List<ProjectionSpan> spans = new List<ProjectionSpan>();

            for (int i = 0; i < this.tokens.Length; i++)
            {
                Token token = this.tokens[i];
                string chunk = this.chunks[i];

                if (token.Symbol is Inert)
                    builder.Append(chunk);
                else if (token.Symbol is Code && this.buffers.TryGetValue(this.chunks[i], out var list))
                {
                    foreach (var (text, span) in list)
                    {
                        if (span != null)
                            spans.Add(new ProjectionSpan(span.Value, builder.Length));

                        builder.Append(text);
                    }
                }
            }

			return new ProjectionResult()
			{
				Content = builder.ToString(),
				Spans = spans.OrderBy(s => s.To.Start).ToList(),
			};
		}

        private class SourceChunk
        {
            public string Text { get; set; }
        }
    }
}
