namespace Maestro
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

	internal delegate void ExternalCommandCallback(ref Context context);

	internal readonly struct ExternalCommandBinding
	{
		public readonly ExternalCommandDefinition definition;
		public readonly System.Func<ExternalCommandCallback> factory;

		public ExternalCommandBinding(ExternalCommandDefinition definition, System.Func<ExternalCommandCallback> factory)
		{
			this.definition = definition;
			this.factory = factory;
		}
	}

	internal readonly struct ExternalCommandInstance
	{
		public readonly int definitionIndex;
		public readonly int sourceIndex;
		public readonly Slice slice;

		public ExternalCommandInstance(int definitionIndex, int sourceIndex, Slice slice)
		{
			this.definitionIndex = definitionIndex;
			this.sourceIndex = sourceIndex;
			this.slice = slice;
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
	}

	public readonly struct CommandDefinition
	{
		public readonly string name;
		public readonly int codeIndex;
		public readonly Slice externalCommandSlice;
		public readonly byte parameterCount;

		public CommandDefinition(string name, int codeIndex, Slice externalCommandSlice, byte parameterCount)
		{
			this.name = name;
			this.codeIndex = codeIndex;
			this.externalCommandSlice = externalCommandSlice;
			this.parameterCount = parameterCount;
		}

		public bool IsEqualTo(ExternalCommandDefinition other)
		{
			return name == other.name && parameterCount == other.parameterCount;
		}
	}
}