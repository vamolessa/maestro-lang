[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("tests")]

namespace Maestro
{
	public sealed class Engine
	{
		internal readonly CompilerController controller = new CompilerController();
		internal readonly VirtualMachine vm = new VirtualMachine();
		internal readonly ExternalCommandBindingRegistry bindingRegistry = new ExternalCommandBindingRegistry();
		internal readonly SourceCollection librarySources = new SourceCollection();

		public void RegisterLibrary(Source source)
		{
			librarySources.AddSource(source);
		}

		public void RegisterCommand<T>(string name, System.Func<ICommand<T>> commandFactory) where T : struct, ITuple
		{
			bindingRegistry.Register(new ExternalCommandBinding(
				new ExternalCommandDefinition(
					name,
					default(T).Size
				),
				() =>
				{
					var command = commandFactory();
					return (ref Context context) =>
					{
						var args = default(T);
						args.Read(context.stack.buffer, context.startIndex + context.inputCount);
						command.Execute(ref context, args);
					};
				}
			));
		}

		public CompileResult CompileSource(Source source, Mode mode)
		{
			var chunk = new ByteCodeChunk();
			var errors = controller.CompileSource(chunk, librarySources, mode, source);

			var instances = EngineHelper.InstantiateExternalCommands(
				bindingRegistry,
				chunk,
				new Slice(0, chunk.externalCommandInstances.count),
				ref errors
			);

			return new CompileResult(errors, new Executable<Tuple0>(chunk, instances, 0));
		}

		public Option<Executable<T>> InstantiateCommand<T>(in CompileResult result, string name) where T : struct, ITuple
		{
			if (result.errors.count > 0)
				return Option.None;

			var parameterCount = default(T).Size;

			var chunk = result.executable.chunk;
			for (var i = 0; i < chunk.commandDefinitions.count; i++)
			{
				var definition = chunk.commandDefinitions.buffer[i];
				if (definition.name != name)
					continue;

				if (definition.parameterCount != parameterCount)
					return Option.None;

				var errors = new Buffer<CompileError>();
				var instances = EngineHelper.InstantiateExternalCommands(
					bindingRegistry,
					chunk,
					definition.externalCommandSlice,
					ref errors
				);

				if (errors.count > 0)
					return Option.None;

				return new Executable<T>(result.executable.chunk, instances, i);
			}

			return Option.None;
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