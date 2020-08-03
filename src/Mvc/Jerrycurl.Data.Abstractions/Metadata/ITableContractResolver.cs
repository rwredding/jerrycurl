namespace Jerrycurl.Data.Metadata
{
    public interface ITableContractResolver
    {
        string[] GetTableName(ITableMetadata metadata);
        string GetColumnName(ITableMetadata metadata);
    }
}
