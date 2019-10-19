using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations;

namespace Jerrycurl.Mvc.Projections
{
    public interface IProjection<out TModel> : IProjection
    {
        new IProjection<TModel> Map(Func<IProjectionAttribute, IProjectionAttribute> m);
        new IProjection<TModel> With(IProjectionMetadata metadata = null,
                                     IEnumerable<IProjectionAttribute> attributes = null,
                                     IField field = null,
                                     IProjectionOptions options = null);
    }
}