using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Test.Conventions.Accessors;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Mvc.Test
{
    public class LocatorTests
    {
        private readonly ProcLocator locator = new ProcLocator();
        private readonly Type accessorType = typeof(LocatorAccessor);

        public void FindPage_CaseInsensitive_ReturnsExpectedPage()
        {
            PageDescriptor query = this.FindPage("locatorquery2");

            query.PageType.ShouldBe(typeof(Conventions.Queries.Locator.LocatorQuery2_cssql));
        }

        public void FindPage_FromAccessor_ReturnsExpectedPage()
        {
            PageDescriptor query = this.FindPage("LocatorQuery2");
            PageDescriptor command = this.FindPage("LocatorCommand1");

            query.PageType.ShouldBe(typeof(Conventions.Queries.Locator.LocatorQuery2_cssql));
            command.PageType.ShouldBe(typeof(Conventions.Commands.Locator.LocatorCommand1_cssql));
        }

        public void FindPage_FromPage_ReturnsExpectedPage()
        {
            PageDescriptor query = this.FindPage("LocatorQuery4", typeof(Conventions.Queries.Locator.LocatorQuery2_cssql));
            PageDescriptor command = this.FindPage("LocatorCommand3", typeof(Conventions.Commands.Locator.LocatorCommand1_cssql));

            query.PageType.ShouldBe(typeof(Conventions.Queries.LocatorQuery4_cssql));
            command.PageType.ShouldBe(typeof(Conventions.Commands.LocatorCommand3_cssql));
        }

        public void FindPage_InSharedRoot_ReturnsExpectedPage()
        {
            PageDescriptor query = this.FindPage("LocatorQuery4");
            PageDescriptor command = this.FindPage("LocatorCommand3");

            query.PageType.ShouldBe(typeof(Conventions.Queries.LocatorQuery4_cssql));
            command.PageType.ShouldBe(typeof(Conventions.Commands.LocatorCommand3_cssql));
        }

        public void FindPage_InSubFolder_ReturnsExpectedPage()
        {
            PageDescriptor query = this.FindPage("SubFolder1/SubFolder2/LocatorQuery1");

            query.PageType.ShouldBe(typeof(Conventions.Queries.Locator.SubFolder1.SubFolder2.LocatorQuery1_cssql));
        }

        public void FindPage_InSharedFolder_ReturnsExpectedPage()
        {
            PageDescriptor query = this.FindPage("LocatorQuery3");
            PageDescriptor command = this.FindPage("LocatorCommand2");

            query.PageType.ShouldBe(typeof(Conventions.Queries.Shared.LocatorQuery3_cssql));
            command.PageType.ShouldBe(typeof(Conventions.Commands.Shared.LocatorCommand2_cssql));
        }

        public void FindPage_IfNotExists_ThrowsExpectedException()
        {
            Should.Throw<PageNotFoundException>(() => this.FindPage("LocatorQueryX", this.accessorType));
        }

        public void FindPage_WithRelativePath_ReturnsExpectedPage()
        {
            PageDescriptor page = this.FindPage("../Queries/Locator/SubFolder1/./SubFolder2/../../LocatorQuery2");

            page.ShouldNotBeNull();
            page.PageType.ShouldBe(typeof(Conventions.Queries.Locator.LocatorQuery2_cssql));
        }

        public void FindPage_WithAbsolutePath_ReturnsExpectedPage()
        {
            PageDescriptor page = this.FindPage("/Jerrycurl/Mvc/Test/Conventions/Queries/Locator/LocatorQuery2.cssql");

            page.ShouldNotBeNull();
            page.PageType.ShouldBe(typeof(Conventions.Queries.Locator.LocatorQuery2_cssql));
        }
        public void FindPage_WithDomainPath_ReturnsExpectedPage()
        {
            PageDescriptor page = this.FindPage("~/Queries/Locator/LocatorQuery2.cssql");

            page.ShouldNotBeNull();
            page.PageType.ShouldBe(typeof(Conventions.Queries.Locator.LocatorQuery2_cssql));
        }

        private PageDescriptor FindPage(string procName, Type originType = null) => this.locator.FindPage(procName, originType ?? this.accessorType);
    }
}
