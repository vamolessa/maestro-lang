namespace Maestro
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
		void OnBegin(VirtualMachine vm);
		void OnEnd(VirtualMachine vm);
		void OnHook(VirtualMachine vm);
	}

	internal sealed class ExternCommandBindingRegistry
	{
		internal Buffer<ExternCommandBinding> bindings = new Buffer<ExternCommandBinding>();

		internal void Register(ExternCommandBinding binding)
		{
			var existingBinding = Find(binding.definition.name);
			if (!existingBinding.isSome)
				bindings.PushBack(binding);
		}

		internal Option<ExternCommandBinding> Find(string name)
		{
			for (var i = 0; i < bindings.count; i++)
			{
				var binding = bindings.buffer[i];
				if (binding.definition.name == name)
					return binding;
			}

			return Option.None;
		}
	}

	internal static class EngineHelper
	{
		internal static ExternCommandCallback[] InstantiateExternCommands(ExternCommandBindingRegistry bindingRegistry, ByteCodeChunk chunk, Slice instancesSlice, ref Buffer<CompileError> errors)
		{
			var instances = new ExternCommandCallback[instancesSlice.length];

			for (var i = 0; i < instancesSlice.length; i++)
			{
				var instance = chunk.externCommandInstances.buffer[i + instancesSlice.index];
				var definition = chunk.externCommandDefinitions.buffer[instance.definitionIndex];

				var binding = bindingRegistry.Find(definition.name);
				if (!binding.isSome)
				{
					errors.PushBack(new CompileError(
						instance.sourceIndex,
						instance.slice,
						new CompileErrors.ExternCommands.ExternCommandHasNoBinding
						{
							name = definition.name
						})
					);
					continue;
				}

				if (binding.value.definition.parameterCount != definition.parameterCount)
				{
					errors.PushBack(new CompileError(
						instance.sourceIndex,
						instance.slice,
						new CompileErrors.ExternCommands.IncompatibleExternCommand
						{
							name = definition.name,
							expectedParameterCount = definition.parameterCount,
							gotParameterCount = binding.value.definition.parameterCount
						})
					);
					continue;
				}

				instances[i] = binding.value.factory();
			}

			return instances;
		}
	}
}