using System.Diagnostics;

namespace Maestro
{
	[DebuggerTypeProxy(typeof(AssemblyDebugView))]
	public sealed class Assembly
	{
		public readonly Source source;
		public Buffer<string> dependencyUris = new Buffer<string>();

		public Buffer<byte> bytes = new Buffer<byte>(256);
		public Buffer<Slice> sourceSlices = new Buffer<Slice>(256);

		public Buffer<Value> literals = new Buffer<Value>(32);
		public Buffer<NativeCommandDefinition> nativeCommandDefinitions = new Buffer<NativeCommandDefinition>(8);
		internal Buffer<NativeCommandInstance> nativeCommandInstances = new Buffer<NativeCommandInstance>(8);
		public Buffer<CommandDefinition> commandDefinitions = new Buffer<CommandDefinition>(8);
		public Buffer<ExternalCommandInstance> externalCommandInstances = new Buffer<ExternalCommandInstance>(8);

		internal Assembly(Source source)
		{
			this.source = source;
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

		internal bool AddNativeCommand(NativeCommandDefinition definition)
		{
			for (var i = 0; i < nativeCommandDefinitions.count; i++)
			{
				if (definition.name == nativeCommandDefinitions.buffer[i].name)
					return false;
			}

			for (var i = 0; i < commandDefinitions.count; i++)
			{
				if (definition.name == commandDefinitions.buffer[i].name)
					return false;
			}

			nativeCommandDefinitions.PushBack(definition);
			return true;
		}

		internal bool AddCommand(CommandDefinition definition)
		{
			for (var i = 0; i < nativeCommandDefinitions.count; i++)
			{
				if (definition.name == nativeCommandDefinitions.buffer[i].name)
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