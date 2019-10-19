using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Mvc;
using Jerrycurl.Test;

namespace Jerrycurl.Extensions.EntityFrameworkCore.Test
{
    public class EntityConvention : DatabaseConvention
    {
        public override void Configure(DomainOptions options)
        {
            options.UseSqlite("FILENAME=ef.db");
            options.UseEntityFrameworkCore<EntityContext>();
        }
    }
}
