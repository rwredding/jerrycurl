using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Commands.Batching
{
    public class BatchedCommand_cssql : ProcPage<object, object>
    {
        public BatchedCommand_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            for (int i = 0; i < 20; i++)
            {
                this.WriteLiteral($"BATCH {i}\r\n");
                this.Write($"Param {i}");

                this.Context.Execution.Buffer.Mark();
            }
        }
    }
}
