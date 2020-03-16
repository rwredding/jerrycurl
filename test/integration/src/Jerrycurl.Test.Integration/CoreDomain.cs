using System;
using Jerrycurl.Mvc;

namespace Jerrycurl.Test.Integration
{
    public class CoreDomain : IDomain
    {
        public void Configure(DomainOptions options)
        {
            Settings.InitializeDomain(options);
        }
    }
}