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

	internal interface ICommand
	{
		void Invoke(Inputs inputs);
	}

	public interface ICommand<A, R>
		where A : struct, ITuple
		where R : struct, ITuple
	{
		R Invoke(Inputs inputs, A args);
	}

	public sealed class CommandWrapper<A, R> : ICommand
		where A : struct, ITuple
		where R : struct, ITuple
	{
		private readonly ICommand<A, R> command;

		public CommandWrapper(ICommand<A, R> command)
		{
			this.command = command;
		}

		public void Invoke(Inputs inputs)
		{
			var args = default(A);
			args.Read(inputs.buffer, inputs.startIndex + inputs.count);
			var ret = command.Invoke(inputs, args);
			ret.Write(inputs.buffer, inputs.startIndex);
		}
	}

	internal readonly struct ExternalCommandBinding
	{
		public readonly ExternalCommandDefinition definition;
		public readonly System.Func<ICommand> factory;

		public ExternalCommandBinding(ExternalCommandDefinition definition, System.Func<ICommand> factory)
		{
			this.definition = definition;
			this.factory = factory;
		}
	}

	internal readonly struct ExternalCommandDefinition
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