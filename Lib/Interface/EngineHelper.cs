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

	internal sealed class ExecutableRegistry
	{
		internal Buffer<Executable> executables = new Buffer<Executable>();

		internal bool Register(Executable executable)
		{
			var registeredExecutable = Find(executable.assembly.source.uri);
			if (registeredExecutable.isSome)
				return false;

			executables.PushBack(executable);
			return true;
		}

		internal Option<Executable> Find(string name)
		{
			for (var i = 0; i < executables.count; i++)
			{
				var executable = executables.buffer[i];
				if (executable.assembly.source.uri == name)
					return executable;
			}

			return Option.None;
		}
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
		internal static Executable[] FindDependencyExecutables(ExecutableRegistry executableRegistry, Assembly assembly, ref Buffer<CompileError> errors)
		{
			if (assembly.dependencyUris.count == 0)
				return null;

			var dependencies = new Executable[assembly.dependencyUris.count];
			for (var i = 0; i < dependencies.Length; i++)
			{
				var uri = assembly.dependencyUris.buffer[i];
				var dependency = executableRegistry.Find(uri);
				if (!dependency.isSome)
				{
					errors.PushBack(new CompileError(new Slice(), new CompileErrors.Assembly.DependencyAssemblyNotFound
					{
						dependencyUri = uri
					}));
				}

				dependencies[i] = dependency.value;
			}

			return dependencies;
		}

		internal static NativeCommandCallback[] InstantiateNativeCommands(NativeCommandBindingRegistry bindingRegistry, Assembly assembly, Slice instancesSlice, ref Buffer<CompileError> errors)
		{
			var instances = new NativeCommandCallback[instancesSlice.length];

			for (var i = 0; i < instancesSlice.length; i++)
			{
				var instance = assembly.nativeCommandInstances.buffer[i + instancesSlice.index];
				var definition = assembly.dependencyNativeCommandDefinitions.buffer[instance.definitionIndex];

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