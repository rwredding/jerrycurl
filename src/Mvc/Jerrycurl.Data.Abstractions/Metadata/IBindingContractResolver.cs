namespace Jerrycurl.Data.Metadata
{
    public interface IBindingContractResolver
    {
        int Priority { get; }

        IBindingParameterContract GetParameterContract(IBindingMetadata metadata);
        IBindingCompositionContract GetCompositionContract(IBindingMetadata metadata);
        IBindingValueContract GetValueContract(IBindingMetadata metadata);
        IBindingHelperContract GetHelperContract(IBindingMetadata metadata);
    }
}
