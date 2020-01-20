namespace Flow
{
	public struct CallFrame
	{
		public enum Type : ushort
		{
			EntryPoint,
			Function,
		}

		public int codeIndex;
		public int baseStackIndex;
		public ushort functionIndex;
		public Type type;

		public CallFrame(int codeIndex, int baseStackIndex, ushort functionIndex, Type type)
		{
			this.codeIndex = codeIndex;
			this.baseStackIndex = baseStackIndex;
			this.functionIndex = functionIndex;
			this.type = type;
		}
	}

	public sealed class VirtualMachine
	{
		public ByteCodeChunk chunk;
		public Buffer<CallFrame> callFrameStack = new Buffer<CallFrame>(4);
		public Buffer<object> stack = new Buffer<object>(32);
		public Buffer<ICommand> commands = new Buffer<ICommand>(8);
		public Buffer<string> localVariableNames = new Buffer<string>(8);

		internal Option<RuntimeError> error;

		internal void Load(ByteCodeChunk chunk)
		{
			this.chunk = chunk;
			callFrameStack.count = 0;
			error = Option.None;

			for (var i = 0; i < chunk.commandInstances.count; i++)
			{
				var index = chunk.commandInstances.buffer[i];
				var command = chunk.commands.buffer[index];
				commands.PushBackUnchecked(command.factory());
			}
		}

		public void Error(IFormattedMessage message)
		{
			var ip = -1;
			if (callFrameStack.count > 0)
				ip = callFrameStack.buffer[callFrameStack.count - 1].codeIndex;

			error = Option.Some(new RuntimeError(
				ip,
				ip >= 0 ? chunk.sourceSlices.buffer[ip] : new Slice(),
				message
			));
		}
	}
}