using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Tools.Info
{
    public abstract class InfoCommand
    {
        public abstract string Vendor { get; }
        public abstract string Connector { get; }
        public abstract string ConnectorVersion { get; }
    }
}
