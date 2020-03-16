using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Locator.SubFolder1.SubFolder2
{
    public class LocatorQuery1_cssql : ProcPage<object, object>
    {
        public LocatorQuery1_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
