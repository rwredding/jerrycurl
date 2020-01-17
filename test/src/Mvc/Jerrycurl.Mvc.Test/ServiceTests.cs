using Jerrycurl.Mvc.Test.Conventions.Accessors;
using Jerrycurl.Mvc.Test.Conventions.DependencyInjection;
using Jerrycurl.Mvc.Test.Conventions.DependencyInjection.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;

namespace Jerrycurl.Mvc.Test
{
    public class ServiceTests
    {
        private readonly ProcLocator locator = new ProcLocator();

        private IServiceProvider GetServiceProvider()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<MyService>();
            services.AddSingleton<DiDomain>();

            return services.BuildServiceProvider();
        }

        public void Test_Projection_WithoutServiceProvider()
        {
            PageDescriptor page = this.locator.FindPage("ProjectedQuery", typeof(MiscAccessor));
            ProcEngine engine = new ProcEngine(null);

            ISqlContent result = engine.Proc(page, new ProcArgs(typeof(object), typeof(object)))(null).Buffer.ReadToEnd();

            result.Text.ShouldBe("PROJEXISTS");
        }

        public void Test_Injection_WithoutServiceProvider()
        {
            PageDescriptor page = this.locator.FindPage("InjectedQuery", typeof(MiscAccessor));
            ProcEngine engine = new ProcEngine(null);

            Should.Throw<NotSupportedException>(() => engine.Proc(page, new ProcArgs(typeof(object), typeof(object)))(null));
        }

        public void Test_ProjectionAndInjection_WithServiceProvider()
        {
            PageDescriptor page = this.locator.FindPage("DiQuery", typeof(DiAccessor));
            ProcEngine engine = new ProcEngine(this.GetServiceProvider());

            ISqlContent result = engine.Proc(page, new ProcArgs(typeof(object), typeof(object)))(null).Buffer.ReadToEnd();

            result.Text.ShouldBe("SOMEVALUE+PROJ");
        }
    }
}
