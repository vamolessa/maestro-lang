namespace Flow
{
	public interface ICommand
	{
		void Run();
	}

	public readonly struct Command
	{
		public readonly string name;
		public readonly System.Func<ICommand> factory;

		public Command(string name, System.Func<ICommand> factory)
		{
			this.name = name;
			this.factory = factory;
		}
	}
}