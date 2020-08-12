using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Commands.Internal
{
    internal interface IFieldSource
    {
        object Value { get; }
        bool HasChanged { get; }
    }
}
