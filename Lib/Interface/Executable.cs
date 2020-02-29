namespace Maestro
{
	public readonly struct Executable
	{
		public readonly Assembly assembly;
		internal readonly NativeCommandCallback[] nativeCommandInstances;

		internal Executable(Assembly assembly, NativeCommandCallback[] nativeCommandInstances)
		{
			this.assembly = assembly;
			this.nativeCommandInstances = nativeCommandInstances;
		}
	}

	public readonly struct ExecuteScope : System.IDisposable
	{
		private readonly VirtualMachine vm;
		private readonly Executable[] executableRegistry;

		internal ExecuteScope(VirtualMachine vm, Executable[] executableRegistry)
		{
			this.vm = vm;
			this.executableRegistry = executableRegistry;
		}

		public int StackCount
		{
			get { return vm.stack.count; }
		}

		public Value this[int index]
		{
			get { return vm.stack.buffer[index]; }
			set { vm.stack.buffer[index] = value; }
		}

		public void PushValue(Value value)
		{
			vm.stack.PushBackUnchecked(value);
		}

		public Value PopValue()
		{
			return vm.stack.PopLast();
		}

		public ExecuteResult Execute(in Executable executable)
		{
			var frameStackIndex = vm.stack.count;

			vm.tupleSizes.count = 0;
			vm.tupleSizes.PushBackUnchecked(frameStackIndex);

			vm.inputSlices.count = 0;
			vm.inputSlices.PushBackUnchecked(new Slice(0, frameStackIndex));

			vm.stackFrames.ZeroClear();
			vm.stackFrames.PushBackUnchecked(new StackFrame(
				executable,
				executable.assembly.bytes.count - 1,
				0,
				-1
			));
			vm.stackFrames.PushBackUnchecked(new StackFrame(
				executable,
				0,
				frameStackIndex,
				-1
			));

			if (vm.debugger.isSome)
				vm.debugger.value.OnBegin(vm);

			var maybeExecuteError = vm.Execute(executable, executableRegistry);

			if (vm.debugger.isSome)
				vm.debugger.value.OnEnd(vm);

			return new ExecuteResult(maybeExecuteError, vm.stackFrames);
		}

		public void Dispose()
		{
			vm.stack.ZeroClear();
			vm.debugInfo.Clear();
		}
	}
}