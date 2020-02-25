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

	internal sealed class AssemblyRegistry
	{
		internal Buffer<Assembly> assemblies = new Buffer<Assembly>();

		internal bool Register(Assembly assembly)
		{
			var existingAssembly = Find(assembly.source.uri);
			if (existingAssembly.isSome)
				return false;

			assemblies.PushBack(assembly);
			return true;
		}

		internal Option<Assembly> Find(string name)
		{
			for (var i = 0; i < assemblies.count; i++)
			{
				var assembly = assemblies.buffer[i];
				if (assembly.source.uri == name)
					return assembly;
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
		internal static Assembly[] FindDependencyAssemblies(AssemblyRegistry assemblyRegistry, Assembly assembly, ref Buffer<CompileError> errors)
		{
			if (assembly.dependencyUris.count == 0)
				return null;

			var dependencies = new Assembly[assembly.dependencyUris.count];
			for (var i = 0; i < dependencies.Length; i++)
			{
				var uri = assembly.dependencyUris.buffer[i];
				var dependency = assemblyRegistry.Find(uri);
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