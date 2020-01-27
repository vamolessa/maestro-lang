namespace Flow
{
	public struct StackFrame
	{
		public int codeIndex;
		public int stackIndex;

		public StackFrame(int codeIndex, int stackIndex)
		{
			this.codeIndex = codeIndex;
			this.stackIndex = stackIndex;
		}
	}

	public readonly struct VariableInfo
	{
		public readonly string name;

		public VariableInfo(string name)
		{
			this.name = name;
		}
	}

	public sealed class VirtualMachine
	{
		internal ByteCodeChunk chunk;
		internal Buffer<StackFrame> stackFrames = new Buffer<StackFrame>(4);
		internal Buffer<Value> stack = new Buffer<Value>(32);
		internal Buffer<ICommand> commands = new Buffer<ICommand>(8);
		internal Buffer<VariableInfo> localVariableInfos = new Buffer<VariableInfo>(8);

		internal Option<RuntimeError> error;

		internal void Load(ByteCodeChunk chunk)
		{
			this.chunk = chunk;
			stackFrames.count = 0;
			error = Option.None;

			for (var i = 0; i < chunk.commandInstances.count; i++)
			{
				var index = chunk.commandInstances.buffer[i];
				var command = chunk.commandDefinitions.buffer[index];
				commands.PushBackUnchecked(command.factory());
			}
		}

		public void Error(IFormattedMessage message)
		{
			var ip = -1;
			if (stackFrames.count > 0)
				ip = stackFrames.buffer[stackFrames.count - 1].codeIndex;

			error = Option.Some(new RuntimeError(
				ip,
				ip >= 0 ? chunk.sourceSlices.buffer[ip] : new Slice(),
				message
			));
		}
	}
}