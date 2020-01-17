using System;
using Fixie;
using Jerrycurl.Mvc;

namespace Jerrycurl.Test
{
    public abstract class DatabaseConvention : Discovery, Execution
    {
        public virtual bool Skip { get; } = false;
        public virtual string SkipReason { get; } = "Test skipped.";

        public abstract void Configure(DomainOptions options);

        public void Execute(TestClass testClass)
        {
            object instance = testClass.Construct();

            testClass.RunCases(@case =>
            {
                if (this.Skip)
                    @case.Skip(this.SkipReason);
                else
                    @case.Execute(instance);
            });

            instance.Dispose();
        }

        protected static string GetEnvironmentVariable(string variableName) => Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine) ??
                                                                               Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User) ??
                                                                               Environment.GetEnvironmentVariable(variableName);
    }
}
