using Microsoft.EntityFrameworkCore;
using Jerrycurl.Extensions.EntityFrameworkCore.Metadata.Builders;

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