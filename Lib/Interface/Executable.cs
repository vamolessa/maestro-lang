namespace Maestro
{
	public readonly struct Executable<T> where T : struct, ITuple
	{
		internal readonly FatAssembly fatAssembly;
		internal readonly NativeCommandCallback[] nativeCommandInstances;
		internal readonly int commandIndex;

		internal Executable(FatAssembly fatAssembly, NativeCommandCallback[] nativeCommandInstances, int commandIndex)
		{
			this.fatAssembly = fatAssembly;
			this.nativeCommandInstances = nativeCommandInstances;
			this.commandIndex = commandIndex;
		}
	}

	public readonly struct ExecuteScope : System.IDisposable
	{
		private readonly VirtualMachine vm;

		internal ExecuteScope(VirtualMachine vm)
		{
			this.vm = vm;
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

		public ExecuteResult Execute<T>(in Executable<T> executable, T args) where T : struct, ITuple
		{
			var command = executable.fatAssembly.assembly.commandDefinitions.buffer[executable.commandIndex];

			var frameStackIndex = vm.stack.count;

			vm.tupleSizes.count = 0;
			vm.tupleSizes.PushBackUnchecked(frameStackIndex);

			vm.inputSlices.count = 0;
			vm.inputSlices.PushBackUnchecked(new Slice(0, frameStackIndex));

			vm.stack.GrowUnchecked(args.Size);
			args.Write(vm.stack.buffer, frameStackIndex);

			vm.stackFrames.ZeroClear();
			vm.stackFrames.PushBackUnchecked(new StackFrame(
				executable.fatAssembly,
				executable.fatAssembly.assembly.bytes.count - 1,
				0,
				0
			));
			vm.stackFrames.PushBackUnchecked(new StackFrame(
				executable.fatAssembly,
				command.codeIndex,
				frameStackIndex,
				executable.commandIndex
			));

			if (vm.debugger.isSome)
				vm.debugger.value.OnBegin(vm);

			var maybeExecuteError = vm.Execute(
				executable.fatAssembly,
				executable.nativeCommandInstances,
				-command.nativeCommandSlice.index
			);

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