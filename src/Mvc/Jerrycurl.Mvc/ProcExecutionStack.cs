using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Mvc
{
    internal class ProcExecutionStack : IProcExecutionStack
    {
        private readonly Stack<PageExecutionContext> stack = new Stack<PageExecutionContext>();

        public bool IsEmpty => (this.stack.Count == 0);
        public IPageExecutionContext Current
        {
            get
            {
                if (this.stack.Count == 0)
                    throw new ProcExecutionException("Execution stack is not initialized. Please add a IPageExecutionContext instance before accessing the stack.");

                return this.stack.Peek();
            }
        }

        public void Push(IPageExecutionContext context)
        {
            IPageExecutionContext currentContext = this.stack.FirstOrDefault();

            PageExecutionContext newContext = new PageExecutionContext()
            {
                Page = context.Page ?? currentContext?.Page,
                Buffer = context.Buffer ?? currentContext?.Buffer,
                Body = context.Body ?? currentContext?.Body,
            };

            this.stack.Push(newContext);
        }

        public IPageExecutionContext Pop() => this.stack.Pop();
    }
}
