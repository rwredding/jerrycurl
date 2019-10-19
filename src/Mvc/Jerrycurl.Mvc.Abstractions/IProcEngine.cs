using Jerrycurl.Mvc.Projections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public delegate IProcResult ProcFactory(object model);
    public delegate void PageFactory(IProjection model, IProjection result);
    public delegate void BodyFactory();

    public interface IProcEngine
    {
        ProcFactory Proc(PageDescriptor descriptor, ProcArgs args);
        PageFactory Page(Type pageType);
        IDomainOptions Options(Type domainType);
    }
}