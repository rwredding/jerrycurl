namespace Jerrycurl.Data.Metadata
{
    public interface IBindingColumnInfo
    {
        ColumnIdentity Column { get; }
        IBindingMetadata Metadata { get; }
        bool CanBeNull { get; }
    }
}