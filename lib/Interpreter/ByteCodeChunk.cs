using System.Diagnostics;

namespace Maestro
{
	[DebuggerTypeProxy(typeof(ByteCodeChunkDebugView))]
	public sealed class ByteCodeChunk
	{
		public Buffer<Source> sources = new Buffer<Source>(0);
		public Buffer<byte> bytes = new Buffer<byte>(256);
		public Buffer<Slice> sourceSlices = new Buffer<Slice>(256);
		public Buffer<int> sourceStartIndexes = new Buffer<int>();

		public Buffer<Value> literals = new Buffer<Value>(32);
		public Buffer<ExternCommandDefinition> externCommandDefinitions = new Buffer<ExternCommandDefinition>(16);
		internal Buffer<ExternCommandInstance> externCommandInstances = new Buffer<ExternCommandInstance>(32);
		public Buffer<CommandDefinition> commandDefinitions = new Buffer<CommandDefinition>(8);

		internal ByteCodeChunk()
		{
			commandDefinitions.PushBackUnchecked(new CommandDefinition("entry-point", 0, new Slice(), 0));
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

		internal bool AddExternCommand(ExternCommandDefinition definition)
		{
			for (var i = 0; i < externCommandDefinitions.count; i++)
			{
				if (definition.name == externCommandDefinitions.buffer[i].name)
					return false;
			}

			for (var i = 0; i < commandDefinitions.count; i++)
			{
				if (definition.name == commandDefinitions.buffer[i].name)
					return false;
			}

			externCommandDefinitions.PushBack(definition);
			return true;
		}

		internal bool AddCommand(CommandDefinition definition)
		{
			for (var i = 0; i < externCommandDefinitions.count; i++)
			{
				if (definition.name == externCommandDefinitions.buffer[i].name)
					return false;
			}

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