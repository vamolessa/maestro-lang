namespace Flow
{
	public struct Context
	{
		public int count;

		internal int startIndex;
		internal Value[] buffer;
		internal IFormattedMessage errorMessage;

		public Value this[int index]
		{
			get { return buffer[startIndex + index]; }
		}

		internal Context(int count, int startIndex, Value[] buffer)
		{
			this.count = count;
			this.startIndex = startIndex;
			this.buffer = buffer;
			this.errorMessage = null;
		}
	}

	public interface ICommand<T>
		where T : struct, ITuple
	{
		void Execute(ref Context context, T args);
	}

	internal delegate void CommandCallback(ref Context context);

	internal readonly struct ExternalCommandBinding
	{
		public readonly ExternalCommandDefinition definition;
		public readonly System.Func<CommandCallback> factory;

		public ExternalCommandBinding(ExternalCommandDefinition definition, System.Func<CommandCallback> factory)
		{
			this.definition = definition;
			this.factory = factory;
		}
	}

	internal readonly struct CommandInstance
	{
		public readonly int definitionIndex;

		public CommandInstance(int definitionIndex)
		{
			this.definitionIndex = definitionIndex;
		}
	}

	public readonly struct ExternalCommandDefinition
	{
		public readonly string name;
		public readonly byte parameterCount;

		public ExternalCommandDefinition(string name, byte parameterCount)
		{
			this.name = name;
			this.parameterCount = parameterCount;
		}

		public bool IsEqualTo(ExternalCommandDefinition other)
		{
			return name == other.name && parameterCount == other.parameterCount;
		}
	}

	public readonly struct CommandDefinition
	{
		public readonly string name;
		public readonly int codeIndex;
		public readonly byte parameterCount;

		public CommandDefinition(string name, int codeIndex, byte parameterCount)
		{
			this.name = name;
			this.codeIndex = codeIndex;
			this.parameterCount = parameterCount;
		}

		public bool IsEqualTo(ExternalCommandDefinition other)
		{
			return name == other.name && parameterCount == other.parameterCount;
		}
	}
}