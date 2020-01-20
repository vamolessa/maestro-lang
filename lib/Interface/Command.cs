namespace Flow
{
	public interface ICommand
	{
		void Run();
	}

	public readonly struct Command
	{
		public readonly string name;
		public readonly byte parameterCount;
		public readonly System.Func<ICommand> factory;

		public Command(string name, byte parameterCount, System.Func<ICommand> factory)
		{
			this.name = name;
			this.parameterCount = parameterCount;
			this.factory = factory;
		}
	}
}