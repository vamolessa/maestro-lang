namespace Flow
{
	public interface ICommand
	{
		void Invoke(Stack stack);
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
	}
}