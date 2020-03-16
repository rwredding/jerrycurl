using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Commands
{
    public class LocatorCommand3_cssql : ProcPage<object, object>
    {
        public LocatorCommand3_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
