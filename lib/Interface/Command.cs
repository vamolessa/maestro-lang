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

	public readonly struct CommandDefinition
	{
		public readonly string name;
		public readonly byte parameterCount;
		public readonly byte returnCount;
		internal readonly System.Func<ICommand> factory;

		internal CommandDefinition(string name, byte parameterCount, byte returnCount, System.Func<ICommand> factory)
		{
			this.name = name;
			this.parameterCount = parameterCount;
			this.returnCount = returnCount;
			this.factory = factory;
		}

		public static CommandDefinition Create<A, R>(string name, System.Func<ICommand<A, R>> factory)
			where A : struct, ITuple
			where R : struct, ITuple
		{
			return new CommandDefinition(
				name,
				default(A).Size,
				default(R).Size,
				() => new CommandWrapper<A, R>(factory())
			);
		}
	}
}