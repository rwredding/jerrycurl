using Jerrycurl.Mvc.Test.Conventions.DependencyInjection.Services;

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
