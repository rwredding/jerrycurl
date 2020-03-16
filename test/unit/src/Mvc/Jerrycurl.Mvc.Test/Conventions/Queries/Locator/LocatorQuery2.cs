using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Locator
{
    public class LocatorQuery2_cssql : ProcPage<object, object>
    {
        public LocatorQuery2_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
