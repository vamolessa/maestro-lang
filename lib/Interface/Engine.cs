using System.Collections.Generic;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("test")]

namespace Flow
{
	public enum Mode
	{
		Release,
		Debug
	}

	public interface IModuleResolver
	{
		Option<string> ResolveModuleSource(Uri requestingSourceUri, Uri moduleUri);
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
				() => new CommandWrapper<A, R>(commandFactory())
			));
		}

		public CompileResult CompileSource(Source source, Mode mode, Option<IModuleResolver> moduleResolver)
		{
			var chunk = new ByteCodeChunk();
			var errors = controller.CompileSource(chunk, /*moduleResolver, mode,*/ source);
			return new CompileResult(errors, chunk, controller.compiledSources);
		}

		public Option<RuntimeError> Execute(CompileResult result)
		{
			if (result.errors.count > 0)
				return vm.NewError(new RuntimeErrors.HasCompileErrors());

			var loadResult = vm.Load(result.chunk, externalCommandRegistry);
			if (loadResult.isSome)
				return loadResult;

			vm.stackFrames.PushBackUnchecked(new StackFrame(0, 0));
			VirtualMachineInstructions.Execute(vm);

			return Option.None;
		}
	}
}