using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Commands.Internal.V11
{
    internal interface IFieldSource
    {
        object Value { get; set; }
        bool HasChanged { get; }
    }
}
