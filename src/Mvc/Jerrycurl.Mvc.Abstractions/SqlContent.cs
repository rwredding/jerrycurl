using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Mvc
{
    public class SqlContent : ISqlContent
    {
        public IEnumerable<IUpdateBinding> Bindings { get; internal set; } = Array.Empty<IUpdateBinding>();
        public IEnumerable<IParameter> Parameters { get; internal set; } = Array.Empty<IParameter>();
        public string Text { get; internal set; } = "";

        public static SqlContent Empty { get; } = new SqlContent();

        public SqlContent()
        {

        }

        public SqlContent Append(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return new SqlContent()
            {
                Text = this.Text + text,
                Bindings = this.Bindings,
                Parameters = this.Parameters,
            };
        }

        public SqlContent Append(IEnumerable<IParameter> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return new SqlContent()
            {
                Text = this.Text,
                Bindings = this.Bindings,
                Parameters = this.Parameters.Concat(parameters),
            };
        }

        public SqlContent Append(IEnumerable<IUpdateBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            return new SqlContent()
            {
                Text = this.Text,
                Bindings = this.Bindings.Concat(bindings),
                Parameters = this.Parameters,
            };
        }

        public SqlContent Append(params IParameter[] parameters) => this.Append((IEnumerable<IParameter>)parameters);
        public SqlContent Append(params IUpdateBinding[] bindings) => this.Append((IEnumerable<IUpdateBinding>)bindings);

        public override string ToString() => this.Text;

        public void WriteTo(ISqlBuffer buffer)
        {
            buffer.Append(this.Text);
            buffer.Append(this.Parameters);
            buffer.Append(this.Bindings);
        }

        ISqlContent ISqlContent.Append(IEnumerable<IParameter> parameters) => this.Append(parameters);
        ISqlContent ISqlContent.Append(IEnumerable<IUpdateBinding> bindings) => this.Append(bindings);
        ISqlContent ISqlContent.Append(string text) => this.Append(text);
        ISqlContent ISqlContent.Append(params IParameter[] parameter) => this.Append(parameter);
        ISqlContent ISqlContent.Append(params IUpdateBinding[] bindings) => this.Append(bindings);
    }
}
