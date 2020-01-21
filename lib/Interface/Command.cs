namespace Flow
{
	public interface ICommand
	{
		Value Invoke(Value input, Value[] args);
	}

	public readonly struct CommandDefinition
	{
		public readonly string name;
		public readonly byte parameterCount;
		public readonly System.Func<ICommand> factory;

		public CommandDefinition(string name, byte parameterCount, System.Func<ICommand> factory)
		{
			this.name = name;
			this.parameterCount = parameterCount;
			this.factory = factory;
		}
	}
}