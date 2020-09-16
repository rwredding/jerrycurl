namespace Jerrycurl.Data.Metadata
{
    public class BindingValueContract : IBindingValueContract
    {
        public BindingValueReader Read { get; set; }
        public BindingValueConverter Convert { get; set; }
    }
}
