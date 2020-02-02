namespace Flow
{
	public struct Context
	{
		public int inputCount;

		internal int startIndex;
		internal Buffer<Value> stack;
		internal IFormattedMessage errorMessage;

		public Value GetInput(int index)
		{
			return stack.buffer[startIndex + index];
		}

		public void PushValue(Value value)
		{
			stack.PushBack(value);
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