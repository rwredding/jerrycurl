using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;

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
