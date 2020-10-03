using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Mvc
{
    public class SqlBuffer : ISqlBuffer
    {
        private readonly List<IParameter> parameters = new List<IParameter>();
        private readonly List<IUpdateBinding> bindings = new List<IUpdateBinding>();
        private readonly StringBuilder text = new StringBuilder();
        private readonly List<SqlOffset> offsets = new List<SqlOffset>();

        public void Append(IEnumerable<IParameter> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.parameters.AddRange(parameters);
        }

        public void Append(IEnumerable<IUpdateBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            this.bindings.AddRange(bindings);
        }

        public void Append(ISqlContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            this.bindings.AddRange(content.Bindings ?? Array.Empty<IUpdateBinding>());
            this.parameters.AddRange(content.Parameters ?? Array.Empty<IParameter>());
            this.text.Append(content.Text);
        }

        public void Append(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            this.text.Append(text);
        }

        public SqlOffset Mark()
        {
            SqlOffset current = this.GetCurrentOffset();

            this.offsets.Add(current);

            return current;
        }

        private SqlOffset GetCurrentOffset()
        {
            return new SqlOffset()
            {
                NumberOfParams = this.parameters.Count,
                NumberOfBindings = this.bindings.Count,
                Text = this.text.Length,
            };
        }

        public ISqlContent ReadToEnd()
        {
            return new SqlContent()
            {
                Bindings = this.bindings,
                Parameters = this.parameters,
                Text = this.text.ToString()
            };
        }

        public IEnumerable<ISqlContent> Read(ISqlOptions options)
        {
            if (options == null || (options.MaxParameters <= 0 && options.MaxSql <= 0) || (options.MaxParameters >= this.parameters.Count && options.MaxSql >= this.text.Length))
            {
                yield return this.ReadToEnd();
                yield break;
            }

            int yieldedParams = 0;
            int yieldedBindings = 0;
            int yieldedText = 0;

            int maxSql = options.MaxSql <= 0 ? int.MaxValue : options.MaxSql;
            int maxParams = options.MaxParameters <= 0 ? int.MaxValue : options.MaxParameters;

            SqlOffset[] offsets = this.offsets.Concat(new[] { this.GetCurrentOffset() }).ToArray();

            for (int i = 0; i < offsets.Length - 1; i++)
            {
                SqlOffset offset = offsets[i];
                SqlOffset nextOffset = offsets[i + 1];

                if (nextOffset.NumberOfParams - yieldedParams > maxParams || nextOffset.Text - yieldedText > maxSql)
                {
                    yield return new SqlContent()
                    {
                        Bindings = this.bindings.Skip(yieldedBindings).Take(offset.NumberOfBindings - yieldedBindings),
                        Parameters = this.parameters.Skip(yieldedParams).Take(offset.NumberOfParams - yieldedParams),
                        Text = this.text.ToString(yieldedText, offset.Text - yieldedText),
                    };

                    yieldedParams += offset.NumberOfParams - yieldedParams;
                    yieldedText += offset.Text - yieldedText;
                    yieldedBindings += offset.NumberOfBindings - yieldedBindings;
                }
            }

            if (yieldedParams < this.parameters.Count || yieldedText < this.text.Length || yieldedBindings < this.bindings.Count)
            {
                string newText = this.text.ToString(yieldedText, this.text.Length - yieldedText);

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    yield return new SqlContent()
                    {
                        Bindings = this.bindings.Skip(yieldedBindings),
                        Parameters = this.parameters.Skip(yieldedParams),
                        Text = newText,
                    };
                }
            }
        }

    }
}
