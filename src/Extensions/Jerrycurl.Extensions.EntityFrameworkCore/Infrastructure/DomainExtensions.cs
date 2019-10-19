using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Extensions.EntityFrameworkCore.Metadata.Builders;
using System.Linq;

namespace Jerrycurl.Mvc
{
    public static class DomainExtensions
    {
        public static void UseEntityFrameworkCore<TContext>(this DomainOptions options)
            where TContext : DbContext, new()
        {
            options.Schemas.AddContract(new EntityFrameworkCoreContractResolver<TContext>());
        }
    }
}