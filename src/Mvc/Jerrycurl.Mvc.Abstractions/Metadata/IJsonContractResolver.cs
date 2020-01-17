namespace Jerrycurl.Mvc.Metadata
{
    public interface IJsonContractResolver
    {
        string GetPropertyName(IJsonMetadata metadata);
    }
}
