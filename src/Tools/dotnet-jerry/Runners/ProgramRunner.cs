using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jerrycurl.Reflection;
using Jerrycurl.Tools.DotNet.Cli.Commands;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal class ProgramRunner
    {
        public RunnerArgs Args { get; }

        public ProgramRunner(RunnerArgs args)
        {
            this.Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public async Task ExecuteAsync()
        {
            if (this.Args.IsProxy)
            {
                ICommandFactory commandFactory = this.GetCommandFactory();

                switch (this.Args.Command)
                {
                    case "scaffold":
                    case "sf":
                        await CommandRunners.ScaffoldAsync(this.Args, commandFactory.GetScaffoldCommand());
                        break;
                    case "run":
                        await CommandRunners.SqlAsync(this.Args, commandFactory.GetScaffoldCommand());
                        break;
                    case "info":
                        CommandRunners.Info(this.Args, commandFactory.GetInfoCommand());
                        break;
                }
            }
            else
            {
                BuildRunner builder = new BuildRunner(this.Args);

                switch (this.Args.Command)
                {
                    case "scaffold":
                    case "sf":
                    case "info":
                    case "run":
                        await builder.ExecuteAsync();
                        break;
                    case "args":
                        CommandRunners.Args(this.Args);
                        break;
                    case "transpile":
                    case "tp":
                        CommandRunners.Transpile(this.Args);
                        break;
                    case "meow":
                        CommandRunners.Meow();
                        break;
                    default:
                        CommandRunners.Help(this.Args);
                        break;
                }
            }
        }

        private ICommandFactory GetCommandFactory()
        {
            Assembly asm;

            try
            {
                asm = Assembly.Load(this.Args.Proxy.PackageName);
            }
            catch (Exception ex)
            {
                throw new RunnerException($"Cannot to load assembly '{this.Args.Proxy.PackageName}'.", ex);
            }

            Type factoryType = asm.GetLoadableTypes().FirstOrDefault(this.IsValidCommandFactory);

            if (factoryType == null)
                throw new RunnerException($"No valid '{this.Args.Command}' command implementation found in assembly '{asm.GetName().Name}'.");

            try
            {
                return (ICommandFactory)Activator.CreateInstance(factoryType);
            }
            catch (Exception ex)
            {
                throw new RunnerException($"Cannot create instance of command '{factoryType.Name}' in assembly '{asm.GetName().Name}'.", ex);
            }
        }

        private bool IsValidCommandFactory(Type type)
        {
            if (!typeof(ICommandFactory).IsAssignableFrom(type) || type.IsAbstract)
                return false;

            ConstructorInfo ctorInfo = type.GetConstructor(Type.EmptyTypes);

            return (ctorInfo != null && ctorInfo.IsPublic);
        }
    }
}
