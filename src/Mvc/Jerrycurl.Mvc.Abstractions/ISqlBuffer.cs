using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using System.Collections.Generic;

namespace Jerrycurl.Mvc
{
    public interface ISqlBuffer
    {
        void Append(IEnumerable<IParameter> parameters);
        void Append(IEnumerable<ICommandBinding> bindings);
        void Append(string text);
        void Append(ISqlContent content);

        SqlOffset Mark();

        IEnumerable<ISqlContent> Read(ISqlOptions options);
        ISqlContent ReadToEnd();
    }
}
