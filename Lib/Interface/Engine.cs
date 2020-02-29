[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("tests")]

namespace Maestro
{
	public sealed class Engine
	{
		internal readonly CompilerController controller = new CompilerController();
		internal readonly VirtualMachine vm = new VirtualMachine();
		internal readonly Buffer<Executable> executableRegistry = new Buffer<Executable>();
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

		public void SetDebugger(Option<IDebugger> debugger)
		{
			vm.debugger = debugger;
		}

		public CompileResult CompileSource(Source source, Mode mode)
		{
			var errors = controller.CompileSource(mode, source, bindingRegistry, out var assembly);
			return new CompileResult(errors, assembly);
		}

		public LinkResult LinkAssembly(Assembly assembly)
		{
			var errors = new Buffer<CompileError>();

			for (var i = 0; i < executableRegistry.count; i++)
			{
				var uri = executableRegistry.buffer[i].assembly.source.uri;
				if (assembly.source.uri == uri)
				{
					errors.PushBack(new CompileError(new Slice(), new CompileErrors.Assembly.DuplicatedAssembly { uri = uri }));
					return new LinkResult(errors, new Executable(assembly, null));
				}
			}

			var dependencies = EngineHelper.FindDependencyExecutables(
				executableRegistry,
				assembly,
				ref errors
			);

			if (errors.count > 0)
				return new LinkResult(errors, new Executable(assembly, null));

			var instances = EngineHelper.InstantiateNativeCommands(
				bindingRegistry,
				assembly,
				new Slice(0, assembly.nativeCommandInstances.count),
				ref errors
			);

			var executable = new Executable(
				assembly,
				instances
			);

			if (errors.count == 0)
				executableRegistry.PushBack(executable);

			return new LinkResult(errors, executable);
		}

		public ExecuteScope ExecuteScope()
		{
			return new ExecuteScope(vm, executableRegistry.buffer);
		}
	}
}