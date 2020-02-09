[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("tests")]

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

		public ExecuteResult Execute<T>(in Executable<T> executable, T args) where T : struct, ITuple
		{
			var command = executable.chunk.commandDefinitions.buffer[executable.commandIndex];

			var frameStackIndex = vm.stack.count;
			vm.stack.GrowUnchecked(args.Size);
			args.Write(vm.stack.buffer, frameStackIndex);

			vm.stackFrames.count = 0;
			vm.stackFrames.PushBackUnchecked(new StackFrame(
				executable.chunk.bytes.count - 1,
				0,
				0
			));
			vm.stackFrames.PushBackUnchecked(new StackFrame(
				command.codeIndex,
				frameStackIndex,
				executable.commandIndex
			));

			vm.tupleSizes.count = 0;
			vm.tupleSizes.PushBackUnchecked(0);

			vm.inputSlices.count = 0;
			vm.inputSlices.PushBackUnchecked(new Slice(frameStackIndex, 0));

			if (vm.debugger.isSome)
				vm.debugger.value.OnBegin(vm);

			var maybeExecuteError = vm.Execute(
				executable.chunk,
				executable.externalCommandInstances,
				-command.externalCommandSlice.index
			);

			if (vm.debugger.isSome)
				vm.debugger.value.OnEnd(vm);

			vm.stack.ZeroClear();
			vm.debugInfo.Clear();

			return new ExecuteResult(maybeExecuteError, executable.chunk, vm.stackFrames);
		}
	}
}