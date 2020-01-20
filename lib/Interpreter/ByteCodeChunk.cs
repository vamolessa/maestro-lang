using System.Diagnostics;

namespace Flow
{
	[DebuggerTypeProxy(typeof(ByteCodeChunkDebugView))]
	public sealed class ByteCodeChunk
	{
		public Buffer<byte> bytes = new Buffer<byte>(256);
		public Buffer<Slice> sourceSlices = new Buffer<Slice>(256);
		public Buffer<int> sourceStartIndexes = new Buffer<int>();

		public Buffer<Command> commands = new Buffer<Command>(16);
		public Buffer<object> literals = new Buffer<object>(32);
		public Buffer<int> commandInstances = new Buffer<int>(32);

		public bool RegisterCommand(Command command)
		{
			for (var i = 0; i < commands.count; i++)
			{
				if (command.name == commands.buffer[i].name)
					return false;
			}

			commands.PushBack(command);
			return true;
		}

		public void WriteByte(byte value, Slice slice)
		{
			bytes.PushBack(value);
			sourceSlices.PushBack(slice);
		}

		public int AddLiteral(object value)
		{
			for (var i = 0; i < literals.count; i++)
			{
				if (value.Equals(literals.buffer[i]))
					return i;
			}

			literals.PushBack(value);
			return literals.count - 1;
		}
	}
}