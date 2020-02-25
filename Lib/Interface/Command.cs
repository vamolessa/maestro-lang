namespace Maestro
{
	public struct Context
	{
		public int inputCount;

		internal int startIndex;
		internal Buffer<Value> stack;
		internal string errorMessage;

		public Value GetInput(int index)
		{
			return stack.buffer[startIndex + index];
		}

		public void PushValue(Value value)
		{
			stack.PushBack(value);
		}

		public void Error(string errorMessage)
		{
			this.errorMessage = errorMessage;
		}
	}

	public interface ICommand<T>
		where T : struct, ITuple
	{
		void Execute(ref Context context, T args);
	}

	internal delegate void NativeCommandCallback(ref Context context);

	internal readonly struct NativeCommandBinding
	{
		public readonly NativeCommandDefinition definition;
		public readonly System.Func<NativeCommandCallback> factory;

		public NativeCommandBinding(NativeCommandDefinition definition, System.Func<NativeCommandCallback> factory)
		{
			this.definition = definition;
			this.factory = factory;
		}
	}

	internal readonly struct NativeCommandInstance
	{
		public readonly int definitionIndex;
		public readonly Slice slice;

		public NativeCommandInstance(int definitionIndex, Slice slice)
		{
			this.definitionIndex = definitionIndex;
			this.slice = slice;
		}
	}

	public readonly struct NativeCommandDefinition
	{
		public readonly string name;
		public readonly byte parameterCount;

		public NativeCommandDefinition(string name, byte parameterCount)
		{
			this.name = name;
			this.parameterCount = parameterCount;
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

		public bool IsEqualTo(NativeCommandDefinition other)
		{
			return name == other.name && parameterCount == other.parameterCount;
		}
	}
}