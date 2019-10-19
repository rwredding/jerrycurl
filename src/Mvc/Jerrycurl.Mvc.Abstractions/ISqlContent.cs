using Jerrycurl.Data;
using Jerrycurl.Data.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface ISqlContent : ISqlWritable
    {
        IEnumerable<ICommandBinding> Bindings { get; }
        IEnumerable<IParameter> Parameters { get; }
        string Text { get; }

        ISqlContent Append(IEnumerable<IParameter> parameters);
        ISqlContent Append(IEnumerable<ICommandBinding> bindings);
        ISqlContent Append(params IParameter[] parameter);
        ISqlContent Append(params ICommandBinding[] bindings);
        ISqlContent Append(string text);
    }
}
