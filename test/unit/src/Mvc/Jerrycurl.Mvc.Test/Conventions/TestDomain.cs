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
