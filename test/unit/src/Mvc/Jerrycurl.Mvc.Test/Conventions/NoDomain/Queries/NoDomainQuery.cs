using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions2.NoDomain.Queries
{
    public class NoDomainQuery_cssql : ProcPage<object, object>
    {
        public NoDomainQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
