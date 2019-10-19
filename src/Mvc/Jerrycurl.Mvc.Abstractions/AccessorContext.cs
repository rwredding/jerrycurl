using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public class AccessorContext
    {
        public IProcLocator Locator { get; }
        public IProcEngine Engine { get; }

        public AccessorContext(IProcLocator locator, IProcEngine engine)
        {
            this.Locator = locator ?? throw new ArgumentNullException(nameof(locator));
            this.Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }
    }
}
