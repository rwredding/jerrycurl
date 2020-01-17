using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Commands.Shared
{
    public class LocatorCommand2_cssql : ProcPage<object, object>
    {
        public LocatorCommand2_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
