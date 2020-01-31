using System.Collections.Generic;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("test")]

namespace Flow
{
	public enum Mode
	{
		Release,
		Debug
	}

	public interface IImportResolver
	{
		Option<string> ResolveSource(Uri requestingSourceUri, Uri importUri);
	}

	public interface IDebugger
	{
		void Reset(Buffer<Source> sources);
		void OnDebugHook();
	}

	internal sealed class ExternalCommandBindingRegistry
	{
		internal Dictionary<string, ExternalCommandBinding> bindings = new Dictionary<string, ExternalCommandBinding>();

		internal void Register(ExternalCommandBinding binding)
		{
			bindings.Add(binding.definition.name, binding);
		}

		internal Option<ExternalCommandBinding> Find(string name)
		{
			return bindings.TryGetValue(name, out var binding) ?
				Option.Some(binding) :
				Option.None;
		}
	}

	public sealed class Engine
	{
		internal readonly CompilerController controller = new CompilerController();
		internal readonly VirtualMachine vm = new VirtualMachine();
		internal readonly ExternalCommandBindingRegistry externalCommandRegistry = new ExternalCommandBindingRegistry();

		public void RegisterCommand<A, R>(string name, System.Func<ICommand<A, R>> commandFactory)
			where A : struct, ITuple
			where R : struct, ITuple
		{
			externalCommandRegistry.Register(new ExternalCommandBinding(
				new ExternalCommandDefinition(
					name,
					default(A).Size,
					default(R).Size
				),
				() =>
				{
					var command = commandFactory();
					return (inputs) =>
					{
						var args = default(A);
						args.Read(inputs.buffer, inputs.startIndex + inputs.count);

						var result = command.Execute(inputs, args);
						result.ok.Write(inputs.buffer, inputs.startIndex);

						return result.error;
					};
				}
			));
		}

		public CompileResult CompileSource(Source source, Mode mode, Option<IImportResolver> importResolver)
		{
			var chunk = new ByteCodeChunk();
			var errors = controller.CompileSource(chunk, importResolver, mode, source);

			return new CompileResult(
				chunk,
				controller.compiledSources,
				errors
			);
		}

		public void SetDebugger(Option<IDebugger> debugger)
		{
			vm.debugger = debugger;
		}

		public ExecuteResult Execute(CompileResult result)
		{
			if (result.HasErrors)
			{
				return new ExecuteResult(new ExecuteResult.Data(
					vm.NewError(new RuntimeErrors.HasCompileErrors()),
					result.chunk,
					result.sources,
					default
				));
			}

			var loadError = vm.Load(result.chunk, externalCommandRegistry);
			if (loadError.isSome)
			{
				return new ExecuteResult(new ExecuteResult.Data(
					loadError.value,
					result.chunk,
					result.sources,
					default
				));
			}

			vm.stackFrames.PushBackUnchecked(new StackFrame(0, 0, -1));
			var executeError = VirtualMachineInstructions.Execute(vm);

			return new ExecuteResult(executeError.isSome ?
				new ExecuteResult.Data(
					executeError.value,
					result.chunk,
					result.sources,
					vm.stackFrames
				) :
				null
			);
		}
	}
}