using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using System.Collections.Generic;

namespace Jerrycurl.Mvc
{
    public interface ISqlContent : ISqlWritable
    {
        IEnumerable<IUpdateBinding> Bindings { get; }
        IEnumerable<IParameter> Parameters { get; }
        string Text { get; }

        ISqlContent Append(IEnumerable<IParameter> parameters);
        ISqlContent Append(IEnumerable<IUpdateBinding> bindings);
        ISqlContent Append(params IParameter[] parameter);
        ISqlContent Append(params IUpdateBinding[] bindings);
        ISqlContent Append(string text);
    }
}
