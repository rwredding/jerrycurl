using System.Linq.Expressions;
using System.Reflection;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingCompositionContract
    {
        MethodInfo Add { get; }
        NewExpression Construct { get; }
        MethodInfo AddDynamic { get; }
    }
}
