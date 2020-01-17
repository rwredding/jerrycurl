namespace Jerrycurl.Data.Metadata
{
    public delegate void BindingParameterWriter(IBindingParameterInfo parameterInfo);
    public delegate object BindingParameterConverter(object value);

    public interface IBindingParameterContract
    {
        BindingParameterWriter Write { get; }
        BindingParameterConverter Convert { get; }
    }
}
