namespace Maestro
{
	public enum Mode
	{
		Release,
		Debug
	}

	public interface IDebugger
	{
		void OnBegin(VirtualMachine vm);
		void OnEnd(VirtualMachine vm);
		void OnHook(VirtualMachine vm);
	}

	internal sealed class NativeCommandBindingRegistry
	{
		internal Buffer<NativeCommandBinding> bindings = new Buffer<NativeCommandBinding>();

		internal bool Register(NativeCommandBinding binding)
		{
			var existingBinding = Find(binding.definition.name);
			if (existingBinding.isSome)
				return false;

			bindings.PushBack(binding);
			return true;
		}

		internal Option<NativeCommandBinding> Find(string name)
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
		internal static void LinkAssembly(Buffer<Executable> executableRegistry, Assembly assembly, ref Buffer<CompileError> errors)
		{
			for (var i = 0; i < assembly.externalCommandInstances.count; i++)
			{
				var instance = assembly.externalCommandInstances.buffer[i];
				for (var j = 0; j < executableRegistry.count; j++)
				{
					var executable = executableRegistry.buffer[j];
					var found = false;
					for (var k = 0; k < executable.assembly.commandDefinitions.count; k++)
					{
						var definition = executable.assembly.commandDefinitions.buffer[k];
						if (instance.name != definition.name || !definition.exported)
							continue;

						if (instance.argumentCount == definition.parameterCount)
						{
							BytesHelper.UShortToBytes(
								(ushort)j,
								out assembly.bytes.buffer[instance.instructionIndex + 1],
								out assembly.bytes.buffer[instance.instructionIndex + 2]
							);
							assembly.bytes.buffer[instance.instructionIndex + 3] = (byte)k;
						}
						else
						{
							errors.PushBack(new CompileError(new Slice(), new CompileErrors.Commands.WrongNumberOfCommandArguments
							{
								commandName = definition.name,
								expected = definition.parameterCount,
								got = instance.argumentCount
							}));
						}
						found = true;
						break;
					}

					if (!found)
					{
						errors.PushBack(new CompileError(new Slice(), new CompileErrors.Commands.CommandNotRegistered { name = instance.name }));
					}
				}
			}
		}

		internal static NativeCommandCallback[] InstantiateNativeCommands(NativeCommandBindingRegistry bindingRegistry, Assembly assembly, ref Buffer<CompileError> errors)
		{
			if (assembly.nativeCommandInstances.count == 0)
				return null;

			var instances = new NativeCommandCallback[assembly.nativeCommandInstances.count];

			for (var i = 0; i < instances.Length; i++)
			{
				var instance = assembly.nativeCommandInstances.buffer[i];
				var definition = assembly.nativeCommandDefinitions.buffer[instance.definitionIndex];

				var binding = bindingRegistry.Find(definition.name);
				if (!binding.isSome)
				{
					errors.PushBack(new CompileError(
						instance.slice,
						new CompileErrors.NativeCommands.NativeCommandHasNoBinding
						{
							name = definition.name
						})
					);
					continue;
				}

				if (binding.value.definition.parameterCount != definition.parameterCount)
				{
					errors.PushBack(new CompileError(
						instance.slice,
						new CompileErrors.NativeCommands.IncompatibleNativeCommand
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