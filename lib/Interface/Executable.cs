namespace Maestro
{
	public readonly struct Executable<T> where T : struct, ITuple
	{
		internal readonly ByteCodeChunk chunk;
		internal readonly ExternalCommandCallback[] externalCommandInstances;
		public readonly int commandIndex;

		internal Executable(ByteCodeChunk chunk, ExternalCommandCallback[] externalCommandInstances, int commandIndex)
		{
			this.chunk = chunk;
			this.externalCommandInstances = externalCommandInstances;
			this.commandIndex = commandIndex;
		}
	}

	public readonly struct ExecuteScope : System.IDisposable
	{
		private readonly VirtualMachine vm;
		private readonly int startIndex;

		internal ExecuteScope(VirtualMachine vm)
		{
			this.vm = vm;
			this.startIndex = vm.stack.count;
		}

		public int StackSize
		{
			get { return vm.stack.count; }
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
			var command = executable.chunk.commandDefinitions.buffer[executable.commandIndex];

			var frameStackIndex = vm.stack.count;
			vm.stack.GrowUnchecked(args.Size);
			args.Write(vm.stack.buffer, frameStackIndex);

			vm.stackFrames.count = 0;
			vm.stackFrames.PushBackUnchecked(new StackFrame(
				executable.chunk.bytes.count - 1,
				0,
				0
			));
			vm.stackFrames.PushBackUnchecked(new StackFrame(
				command.codeIndex,
				frameStackIndex,
				executable.commandIndex
			));

			vm.tupleSizes.count = 0;
			vm.tupleSizes.PushBackUnchecked(0);

			vm.inputSlices.count = 0;
			vm.inputSlices.PushBackUnchecked(new Slice(frameStackIndex, 0));

			if (vm.debugger.isSome)
				vm.debugger.value.OnBegin(vm);

			var maybeExecuteError = vm.Execute(
				executable.chunk,
				executable.externalCommandInstances,
				-command.externalCommandSlice.index
			);

			if (vm.debugger.isSome)
				vm.debugger.value.OnEnd(vm);

			vm.stack.ZeroClear();
			vm.debugInfo.Clear();

			return new ExecuteResult(maybeExecuteError, executable.chunk, vm.stackFrames);
		}

		public void Dispose()
		{
		}
	}
}