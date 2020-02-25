[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("tests")]

namespace Maestro
{
	public sealed class Engine
	{
		internal readonly CompilerController controller = new CompilerController();
		internal readonly VirtualMachine vm = new VirtualMachine();
		internal readonly AssemblyRegistry assemblyRegistry = new AssemblyRegistry();
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
			var errors = controller.CompileSource(mode, source, assemblyRegistry, bindingRegistry, out var assembly);

			var dependencies = EngineHelper.FindDependencyAssemblies(
				assemblyRegistry,
				assembly,
				ref errors
			);

			var instances = EngineHelper.InstantiateNativeCommands(
				bindingRegistry,
				assembly,
				new Slice(0, assembly.nativeCommandInstances.count),
				ref errors
			);

			var fa = new FatAssembly(assembly, dependencies);

			if (errors.count == 0)
				assemblyRegistry.Register(fa);

			return new CompileResult(errors, new Executable<Tuple0>(fa, instances, 0));
		}

		public Option<Executable<T>> InstantiateCommand<T>(in CompileResult result, string name) where T : struct, ITuple
		{
			if (result.errors.count > 0)
				return Option.None;

			var parameterCount = default(T).Size;

			var assembly = result.executable.fatAssembly.assembly;
			for (var i = 0; i < assembly.commandDefinitions.count; i++)
			{
				var definition = assembly.commandDefinitions.buffer[i];
				if (definition.name != name)
					continue;

				if (definition.parameterCount != parameterCount)
					return Option.None;

				var errors = new Buffer<CompileError>();
				var instances = EngineHelper.InstantiateNativeCommands(
					bindingRegistry,
					assembly,
					definition.nativeCommandSlice,
					ref errors
				);

				if (errors.count > 0)
					return Option.None;

				return new Executable<T>(result.executable.fatAssembly, instances, i);
			}

			return Option.None;
		}

		public void SetDebugger(IDebugger debugger)
		{
			vm.debugger = Option.Some(debugger);
		}

		public ExecuteScope ExecuteScope()
		{
			return new ExecuteScope(vm);
		}
	}
}