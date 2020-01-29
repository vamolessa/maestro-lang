namespace Flow
{
	public readonly struct Inputs
	{
		public readonly int count;

		internal readonly int startIndex;
		internal readonly Value[] buffer;

		public Value this[int index]
		{
			get { return buffer[startIndex + index]; }
		}

		internal Inputs(int count, int startIndex, Value[] buffer)
		{
			this.count = count;
			this.startIndex = startIndex;
			this.buffer = buffer;
		}
	}

	public interface ICommand<A, R>
		where A : struct, ITuple
		where R : struct, ITuple
	{
		R Execute(Inputs inputs, A args);
	}

	internal delegate void CommandCallback(Inputs inputs);

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

	public readonly struct ExternalCommandDefinition
	{
		public readonly string name;
		public readonly byte parameterCount;
		public readonly byte returnCount;

		public ExternalCommandDefinition(string name, byte parameterCount, byte returnCount)
		{
			this.name = name;
			this.parameterCount = parameterCount;
			this.returnCount = returnCount;
		}

		public bool IsEqualTo(ExternalCommandDefinition other)
		{
			return
				name == other.name &&
				parameterCount == other.parameterCount &&
				returnCount == other.returnCount;
		}
	}
}