using System.Linq.Expressions;
using System.Reflection;

namespace Jerrycurl.Data.Metadata
{
    public class BindingCompositionContract : IBindingCompositionContract
    {
        public MethodInfo Add { get; set; }
        public NewExpression Construct { get; set; }
        public MethodInfo AddDynamic { get; set; }
    }
}
