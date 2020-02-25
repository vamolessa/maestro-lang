namespace Maestro.StdLib
{
	public static class EngineOperationCommandsExtensions
	{
		public static void RegisterOperationCommands(this Engine engine)
		{
			engine.RegisterSingletonCommand("and", new AndCommand());
			engine.RegisterSingletonCommand("or", new OrCommand());
			engine.RegisterSingletonCommand("not", new NotCommand());

			engine.RegisterSingletonCommand("+", new AddCommand());
			engine.RegisterSingletonCommand("-", new SubtractCommand());
			engine.RegisterSingletonCommand("*", new MultiplyCommand());
			engine.RegisterSingletonCommand("/", new DivideCommand());

			engine.RegisterSingletonCommand("<", new LessThanCommand());
			engine.RegisterSingletonCommand(">", new GreaterThanCommand());
			engine.RegisterSingletonCommand("=", new EqualsCommand());
		}
	}
}
