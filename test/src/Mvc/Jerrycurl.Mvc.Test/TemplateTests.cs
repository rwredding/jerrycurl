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
    public class TemplateTests
    {
        public void Proc_UsesTemplatesCorrectly()
        {
            MiscAccessor misc = new MiscAccessor();

            IList<int> result = misc.TemplatedQuery();

            result.ShouldBe(new[] { 1, 2, 3 });
        }

        public void Proc_UsesPartialsCorrectly()
        {
            MiscAccessor misc = new MiscAccessor();

            IList<int> result = misc.PartialedQuery();

            result.ShouldBe(new[] { 1, 2, 3 });
        }
    }
}
