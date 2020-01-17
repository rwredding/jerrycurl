using System;

namespace Jerrycurl.Data.Metadata
{
    public class BindingHelperContract<THelper> : IBindingHelperContract
        where THelper : class
    {
        public BindingHelperContract(THelper helper)
        {
            this.Object = helper;
            this.Type = typeof(THelper);
        }

        public object Object { get; }
        public Type Type { get; }
    }
}
