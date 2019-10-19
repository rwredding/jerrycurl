using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands
{
    public interface ICommandBinding
    {
        IField Field { get; }
    }
}
