using System.Linq.Expressions;
using System.Reflection;

namespace Jerrycurl.Data.Metadata
{
    public delegate Expression BindingValueConverter(IBindingValueInfo valueInfo);
    public delegate MethodInfo BindingColumnReader(IBindingColumnInfo columnInfo);

    public interface IBindingValueContract
    {
        BindingColumnReader Read { get; }
        BindingValueConverter Convert { get; }
    }
}
