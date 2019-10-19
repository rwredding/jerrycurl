using Jerrycurl.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Mvc.Test.Conventions
{
    public class TestDomain : IDomain
    {
        public void Configure(DomainOptions options)
        {
            options.UseSqlite("DATA SOURCE=testmvc.db");
        }
    }
}
