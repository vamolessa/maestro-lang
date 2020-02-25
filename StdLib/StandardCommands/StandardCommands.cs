namespace Maestro.StdLib
{
	public static class EngineStandardCommandsExtensions
	{
		public static void RegisterStandardCommands(this Engine engine, System.Action<string> writter)
		{
			engine.RegisterSingletonCommand("print", new PrintCommand(writter));
			engine.RegisterSingletonCommand("error", new ErrorCommand());

			engine.RegisterSingletonCommand("count", new CountCommand());
			engine.RegisterSingletonCommand("append", new AppendCommand());
			engine.RegisterSingletonCommand("at", new AtCommand());
			engine.RegisterSingletonCommand("take", new TakeCommand());
			engine.RegisterSingletonCommand("enumerate", new EnumerateCommand());
		}
	}
}
