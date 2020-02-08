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

	internal delegate void ExternCommandCallback(ref Context context);

	internal readonly struct ExternCommandBinding
	{
		public readonly ExternCommandDefinition definition;
		public readonly System.Func<ExternCommandCallback> factory;

		public ExternCommandBinding(ExternCommandDefinition definition, System.Func<ExternCommandCallback> factory)
		{
			this.definition = definition;
			this.factory = factory;
		}
	}

	internal readonly struct ExternCommandInstance
	{
		public readonly int definitionIndex;
		public readonly int sourceIndex;
		public readonly Slice slice;

		public ExternCommandInstance(int definitionIndex, int sourceIndex, Slice slice)
		{
			this.definitionIndex = definitionIndex;
			this.sourceIndex = sourceIndex;
			this.slice = slice;
		}
	}

	public readonly struct ExternCommandDefinition
	{
		public readonly string name;
		public readonly byte parameterCount;

		public ExternCommandDefinition(string name, byte parameterCount)
		{
			this.name = name;
			this.parameterCount = parameterCount;
		}
	}

	public readonly struct CommandDefinition
	{
		public readonly string name;
		public readonly int codeIndex;
		public readonly Slice externCommandSlice;
		public readonly byte parameterCount;

		public CommandDefinition(string name, int codeIndex, Slice externCommandSlice, byte parameterCount)
		{
			this.name = name;
			this.codeIndex = codeIndex;
			this.externCommandSlice = externCommandSlice;
			this.parameterCount = parameterCount;
		}

		public bool IsEqualTo(ExternCommandDefinition other)
		{
			return name == other.name && parameterCount == other.parameterCount;
		}
	}
}