using System.Diagnostics;

namespace Flow
{
	[DebuggerTypeProxy(typeof(ByteCodeChunkDebugView))]
	public sealed class ByteCodeChunk
	{
		public Buffer<byte> bytes = new Buffer<byte>(256);
		public Buffer<Slice> sourceSlices = new Buffer<Slice>(256);
		public Buffer<int> sourceStartIndexes = new Buffer<int>();

		public Buffer<Value> literals = new Buffer<Value>(32);
		public Buffer<ExternalCommandDefinition> externalCommandDefinitions = new Buffer<ExternalCommandDefinition>(16);
		internal Buffer<CommandInstance> externalCommandInstances = new Buffer<CommandInstance>(32);
		public Buffer<CommandDefinition> commandDefinitions = new Buffer<CommandDefinition>(8);
		internal Buffer<CommandInstance> commandInstances = new Buffer<CommandInstance>(8);

		internal ByteCodeChunk()
		{
			commandDefinitions.PushBackUnchecked(new CommandDefinition("entry-point", 0, 0, 0));
			commandInstances.PushBackUnchecked(new CommandInstance(0, 0));
		}

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

		internal bool AddExternalCommand(ExternalCommandDefinition definition)
		{
			for (var i = 0; i < externalCommandDefinitions.count; i++)
			{
				if (definition.name == externalCommandDefinitions.buffer[i].name)
					return false;
			}

			externalCommandDefinitions.PushBack(definition);
			return true;
		}

		internal bool AddCommand(CommandDefinition definition)
		{
			for (var i = 0; i < commandDefinitions.count; i++)
			{
				if (definition.name == commandDefinitions.buffer[i].name)
					return false;
			}

			commandDefinitions.PushBack(definition);
			return true;
		}
	}
}