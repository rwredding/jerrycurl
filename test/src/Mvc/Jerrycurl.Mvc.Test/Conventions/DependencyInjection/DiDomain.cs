using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Test.Conventions.DependencyInjection.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Mvc.Test.Conventions.DependencyInjection
{
    public class DiDomain : IDomain
    {
        private readonly MyService service;

        public DiDomain(MyService service)
        {
            this.service = service;
        }

        public void Configure(DomainOptions options)
        {
            options.UseSqlite(this.service.ConnectionString);
        }
    }
}
