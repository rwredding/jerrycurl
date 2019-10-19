using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface IServiceResolver
    {
        TService GetService<TService>() where TService : class;
        IProjection<TModel> GetProjection<TModel>(IProcContext context);
    }
}
