using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Test.Conventions.Accessors;
using Jerrycurl.Mvc.Test.Conventions2.NoDomain;
using Jerrycurl.Mvc.Test.Conventions2.NoDomain.Queries;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Mvc.Test
{
    public class EngineTests
    {
        private readonly ProcLocator locator = new ProcLocator();
        private readonly ProcEngine engine = new ProcEngine();

        public void Page_CanResolveFactory_WithoutDomain()
        {
            PageDescriptor descriptor = this.locator.FindPage("NoDomainQuery", typeof(NoDomainAccessor));
            ProcArgs args = new ProcArgs(typeof(object), typeof(object));

            descriptor.ShouldNotBeNull();
            descriptor.DomainType.ShouldBeNull();

            PageFactory factory = Should.NotThrow(() => this.engine.Page(descriptor.PageType));

            factory.ShouldNotBeNull();
        }

        public void Proc_IfNoDomainAssociated_ThrowsExpectedException()
        {
            PageDescriptor descriptor = this.locator.FindPage("NoDomainQuery", typeof(NoDomainAccessor));
            ProcArgs args = new ProcArgs(typeof(object), typeof(object));

            descriptor.ShouldNotBeNull();
            descriptor.DomainType.ShouldBeNull();


            Should.Throw<ProcExecutionException>(() => this.engine.Proc(descriptor, args));
        }

        public void Page_IfExists_ReturnsFactory()
        {
            PageDescriptor descriptor = this.FindPage("LocatorQuery2");
            ProcArgs args = new ProcArgs(typeof(int), typeof(object));

            ProcFactory factory = this.engine.Proc(descriptor, args);

            factory.ShouldNotBeNull();
        }

        public void Page_WithResultObject_CanConvertFromAnyType()
        {
            PageDescriptor descriptor = this.FindPage("LocatorQuery2");
            ProcArgs args1 = new ProcArgs(typeof(object), typeof(int));
            ProcArgs args2 = new ProcArgs(typeof(object), typeof(string));

            this.engine.Proc(descriptor, args1).ShouldNotBeNull();
            this.engine.Proc(descriptor, args2).ShouldNotBeNull();
        }

        private PageDescriptor FindPage(string procName) => this.locator.FindPage(procName, typeof(LocatorAccessor));
    }
}
