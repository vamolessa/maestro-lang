[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("test")]

namespace Maestro
{
	public sealed class Engine
	{
		internal readonly CompilerController controller = new CompilerController();
		internal readonly VirtualMachine vm = new VirtualMachine();
		internal readonly ExternalCommandBindingRegistry bindingRegistry = new ExternalCommandBindingRegistry();

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

		public CompileResult CompileSource(Source source, Mode mode, Option<IImportResolver> importResolver)
		{
			var chunk = new ByteCodeChunk();
			var errors = controller.CompileSource(chunk, importResolver, mode, source);

			var instances = EngineHelper.InstantiateExternalCommands(bindingRegistry, chunk, ref errors);

			var maybeExecutable = errors.count == 0 ?
				Option.Some(new Executable(chunk, instances, controller.compiledSources.buffer)) :
				default;
			return new CompileResult(errors, maybeExecutable);
		}

		public Option<bool> InstantiateCommand<T>(CompileResult result, string name) where T : struct, ITuple
		{
			if (!result.executable.isSome)
				return Option.None;

			var parameterCount = default(T).Size;

			var chunk = result.executable.value.chunk;
			for (var i = 0; i < chunk.commandDefinitions.count; i++)
			{
				var definition = chunk.commandDefinitions.buffer[i];
				if (
					definition.name == name &&
					definition.parameterCount == parameterCount
				)
				{
					return true;
				}
			}

			return Option.None;
		}

		public void SetDebugger(Option<IDebugger> debugger)
		{
			vm.debugger = debugger;
		}

		public ExecuteResult Execute(Executable executable)
		{
			vm.stackFrames.count = 0;
			vm.stackFrames.PushBackUnchecked(new StackFrame(0, 0, 0));
			var executeError = vm.Execute(executable);
			vm.stack.ZeroClear();
			vm.debugInfo.Clear();

			return new ExecuteResult(executeError.isSome ?
				new ExecuteResult.Data(
					executeError.value,
					executable.chunk,
					executable.sources,
					vm.stackFrames
				) :
				null
			);
		}
	}
}