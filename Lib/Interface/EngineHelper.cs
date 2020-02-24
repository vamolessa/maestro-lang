namespace Maestro
{
	public enum Mode
	{
		Release,
		Debug
	}

	public interface IDebugger
	{
		void OnBegin(VirtualMachine vm, Assembly assembly);
		void OnEnd(VirtualMachine vm, Assembly assembly);
		void OnHook(VirtualMachine vm, Assembly assembly);
	}

	internal sealed class ExternalCommandBindingRegistry
	{
		internal Buffer<ExternalCommandBinding> bindings = new Buffer<ExternalCommandBinding>();

		internal bool Register(ExternalCommandBinding binding)
		{
			var existingBinding = Find(binding.definition.name);
			if (existingBinding.isSome)
				return false;

			bindings.PushBack(binding);
			return true;
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
		internal static ExternalCommandCallback[] InstantiateExternalCommands(ExternalCommandBindingRegistry bindingRegistry, Assembly assembly, Slice instancesSlice, ref Buffer<CompileError> errors)
		{
			var instances = new ExternalCommandCallback[instancesSlice.length];

			for (var i = 0; i < instancesSlice.length; i++)
			{
				var instance = assembly.externalCommandInstances.buffer[i + instancesSlice.index];
				var definition = assembly.externalCommandDefinitions.buffer[instance.definitionIndex];

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