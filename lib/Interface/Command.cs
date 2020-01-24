namespace Flow
{
	public interface ICommand
	{
		void Invoke(VirtualMachine vm, int inputCount);
	}

	public interface ICommand<A, R>
		where A : struct, ITuple
		where R : struct, ITuple
	{
		R Invoke(VirtualMachine vm, int inputCount, A args);
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

		public void Invoke(VirtualMachine vm, int inputCount)
		{
			var args = default(A);
			args.FromStack(vm, vm.stack.count);
			var ret = command.Invoke(vm, inputCount, args);
			ret.PushToStack(vm);
		}
	}

	public readonly struct CommandDefinition
	{
		public readonly string name;
		public readonly byte parameterCount;
		public readonly byte returnCount;
		public readonly System.Func<ICommand> factory;

		public CommandDefinition(string name, byte parameterCount, byte returnCount, System.Func<ICommand> factory)
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