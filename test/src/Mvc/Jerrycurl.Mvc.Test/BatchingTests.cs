using Jerrycurl.Data.Commands;
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
    public class BatchingTests
    {
        private readonly ProcLocator locator = new ProcLocator();
        private readonly ProcEngine engine = new ProcEngine();

        public void SqlBuffer_Batching_HasCorrectValue()
        {
            PageDescriptor page = this.locator.FindPage("../Commands/Batching/BatchedCommand.cssql", typeof(LocatorAccessor));
            ProcFactory factory = this.engine.Proc(page, new ProcArgs(typeof(object), typeof(object)));

            IProcResult result = factory(null);

            ISqlSerializer<CommandData> serializer = result.Buffer as ISqlSerializer<CommandData>;

            IList<CommandData> batchedBySql = serializer.Serialize(new SqlOptions() { MaxSql = 1 }).ToList();
            IList<CommandData> batchedByParams = serializer.Serialize(new SqlOptions() { MaxParameters = 2 }).ToList();
            IList<CommandData> notBatched = serializer.Serialize(new SqlOptions()).ToList();

            batchedBySql.ShouldNotBeNull();
            batchedByParams.ShouldNotBeNull();
            notBatched.ShouldNotBeNull();

            batchedBySql.Count.ShouldBe(20);
            batchedByParams.Count.ShouldBe(10);
            notBatched.Count.ShouldBe(1);

            string joinedSql = string.Join("", batchedBySql.Select(d => d.CommandText));
            string joinedParams = string.Join("", batchedByParams.Select(d => d.CommandText));

            notBatched.First().CommandText.ShouldBe(joinedSql);
            notBatched.First().CommandText.ShouldBe(joinedParams);

        }
    }
}
