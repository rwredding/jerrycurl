using Jerrycurl.Data.Filters;
using System.Collections.Generic;

namespace Jerrycurl.Mvc
{
    public interface ISqlOptions
    {
        IReadOnlyCollection<IFilter> Filters { get; }

        /// <summary>
        /// The maximum number of bytes each generated ADO.NET command can contain.
        /// </summary>
        int MaxSql { get; set; }

        /// <summary>
        /// The maximum number of parameters each generated ADO.NET command can contain.
        /// </summary>
        int MaxParameters { get; set; }
    }
}
