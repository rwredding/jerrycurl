using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Mvc
{
    /// <summary>
    /// Defines a contract for configuring a Jerrycurl domain in the containing namespace.
    /// </summary>
    public interface IDomain
    {
        /// <summary>
        /// Configures shared options for every query and command executed within the context of the domain. Can be used to configure database vendor, connection string, filters, custom metadata contracts etc.
        /// </summary>
        /// <param name="options">A configurable <see cref="DomainOptions"/> instance.</param>
        void Configure(DomainOptions options);
    }
}
