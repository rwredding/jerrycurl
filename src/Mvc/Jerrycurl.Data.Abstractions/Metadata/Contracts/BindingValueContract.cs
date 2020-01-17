namespace Jerrycurl.Data.Metadata
{
    public class BindingValueContract : IBindingValueContract
    {
        public BindingColumnReader Read { get; set; }
        public BindingValueConverter Convert { get; set; }
    }
}
