namespace Jerrycurl.Data.Metadata
{
    public interface IBindingColumnInfo
    {
        ColumnMetadata Column { get; }
        IBindingMetadata Metadata { get; }
        bool CanBeNull { get; }
    }
}