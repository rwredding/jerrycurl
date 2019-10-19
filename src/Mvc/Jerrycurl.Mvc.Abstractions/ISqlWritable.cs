using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface ISqlWritable
    {
        void WriteTo(ISqlBuffer buffer);
    }
}
