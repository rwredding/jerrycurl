namespace Jerrycurl.Data.Metadata
{
    public interface IBindingColumnInfo
    {
        ColumnInfo Column { get; }
        IBindingMetadata Metadata { get; }
        bool CanBeNull { get; }
    }
}