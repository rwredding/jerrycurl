namespace Jerrycurl.Data.Metadata
{
    internal class BindingColumnInfo : IBindingColumnInfo
    {
        public ColumnInfo Column { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public bool CanBeNull { get; set; }
    }
}
