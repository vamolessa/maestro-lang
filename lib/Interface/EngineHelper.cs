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
		void Reset(Buffer<Source> sources);
		void OnDebugHook();
	}

	internal sealed class ExternalCommandBindingRegistry
	{
		internal Buffer<ExternalCommandBinding> bindings = new Buffer<ExternalCommandBinding>();

		internal void Register(ExternalCommandBinding binding)
		{
			var existingBinding = Find(binding.definition.name);
			if (!existingBinding.isSome)
				bindings.PushBack(binding);
		}

		internal Option<ExternalCommandBinding> Find(string name)
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
		internal static ExternalCommandCallback[] InstantiateExternalCommands(ExternalCommandBindingRegistry bindingRegistry, ByteCodeChunk chunk, Slice instancesSlice, ref Buffer<CompileError> errors)
		{
			var instances = new ExternalCommandCallback[instancesSlice.length];

			for (var i = 0; i < instancesSlice.length; i++)
			{
				var instance = chunk.externalCommandInstances.buffer[i + instancesSlice.index];
				var definition = chunk.externalCommandDefinitions.buffer[instance.definitionIndex];

				var binding = bindingRegistry.Find(definition.name);
				if (!binding.isSome)
				{
					errors.PushBack(new CompileError(
						instance.sourceIndex,
						instance.slice,
						new CompileErrors.ExternalCommands.ExternalCommandHasNoBinding
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
						new CompileErrors.ExternalCommands.IncompatibleExternalCommand
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