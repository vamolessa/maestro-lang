using System.Diagnostics;

namespace Flow
{
	[DebuggerTypeProxy(typeof(ByteCodeChunkDebugView))]
	internal sealed class ByteCodeChunk
	{
		internal Buffer<byte> bytes = new Buffer<byte>(256);
		internal Buffer<Slice> sourceSlices = new Buffer<Slice>(256);
		internal Buffer<int> sourceStartIndexes = new Buffer<int>();

		internal Buffer<ExternalCommandDefinition> commandDefinitions = new Buffer<ExternalCommandDefinition>(16);
		internal Buffer<int> commandInstances = new Buffer<int>(32);
		internal Buffer<Value> literals = new Buffer<Value>(32);

		internal void WriteByte(byte value, Slice slice)
		{
			bytes.PushBack(value);
			sourceSlices.PushBack(slice);
		}

		internal int AddLiteral(Value value)
		{
			for (var i = 0; i < literals.count; i++)
			{
				if (value.IsEqualTo(literals.buffer[i]))
					return i;
			}

			literals.PushBack(value);
			return literals.count - 1;
		}

		internal bool AddExternalCommand(ExternalCommandDefinition command)
		{
			for (var i = 0; i < commandDefinitions.count; i++)
			{
				if (command.name == commandDefinitions.buffer[i].name)
					return false;
			}

			commandDefinitions.PushBack(command);
			return true;
		}
	}
}