[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("tests")]

namespace Maestro
{
	public sealed class Engine
	{
		internal readonly CompilerController controller = new CompilerController();
		internal readonly VirtualMachine vm = new VirtualMachine();
		internal readonly ExecutableRegistry executableRegistry = new ExecutableRegistry();
		internal readonly NativeCommandBindingRegistry bindingRegistry = new NativeCommandBindingRegistry();

		public bool RegisterSingletonCommand<T>(string name, ICommand<T> command) where T : struct, ITuple
		{
			return RegisterCommand(name, () => command);
		}

		public bool RegisterCommand<T>(string name, System.Func<ICommand<T>> commandFactory) where T : struct, ITuple
		{
			return bindingRegistry.Register(new NativeCommandBinding(
				new NativeCommandDefinition(
					name,
					default(T).Size
				),
				() => {
					var command = commandFactory();
					return (ref Context context) => {
						var args = default(T);
						args.Read(context.stack.buffer, context.startIndex + context.inputCount);
						command.Execute(ref context, args);
					};
				}
			));
		}

		public CompileResult CompileSource(Source source, Mode mode)
		{
			var errors = controller.CompileSource(mode, source, executableRegistry, bindingRegistry, out var assembly);

			var dependencies = EngineHelper.FindDependencyExecutables(
				executableRegistry,
				assembly,
				ref errors
			);

			var instances = EngineHelper.InstantiateNativeCommands(
				bindingRegistry,
				assembly,
				new Slice(0, assembly.nativeCommandInstances.count),
				ref errors
			);

			var executable = new Executable(
				assembly,
				dependencies,
				instances
			);

			if (errors.count == 0)
				executableRegistry.Register(executable);

			return new CompileResult(errors, executable);
		}

		public void SetDebugger(Option<IDebugger> debugger)
		{
			vm.debugger = debugger;
		}

		public ExecuteScope ExecuteScope()
		{
			return new ExecuteScope(vm);
		}
	}
}