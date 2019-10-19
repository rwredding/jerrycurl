using Jerrycurl.Data;
using Jerrycurl.Data.Commands;
using Jerrycurl.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Mvc
{
    public class SqlContent : ISqlContent
    {
        public IEnumerable<ICommandBinding> Bindings { get; internal set; } = Array.Empty<ICommandBinding>();
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

        public SqlContent Append(IEnumerable<ICommandBinding> bindings)
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
        public SqlContent Append(params ICommandBinding[] bindings) => this.Append((IEnumerable<ICommandBinding>)bindings);

        public override string ToString() => this.Text;

        public void WriteTo(ISqlBuffer buffer)
        {
            buffer.Append(this.Text);
            buffer.Append(this.Parameters);
            buffer.Append(this.Bindings);
        }

        ISqlContent ISqlContent.Append(IEnumerable<IParameter> parameters) => this.Append(parameters);
        ISqlContent ISqlContent.Append(IEnumerable<ICommandBinding> bindings) => this.Append(bindings);
        ISqlContent ISqlContent.Append(string text) => this.Append(text);
        ISqlContent ISqlContent.Append(params IParameter[] parameter) => this.Append(parameter);
        ISqlContent ISqlContent.Append(params ICommandBinding[] bindings) => this.Append(bindings);
    }
}
