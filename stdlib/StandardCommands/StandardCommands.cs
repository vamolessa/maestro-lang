namespace Maestro.StdLib
{
	public static class EngineStandardCommandsExtensions
	{
		public static void RegisterStandardCommands(this Engine engine)
		{
			engine.RegisterLibrary(new Source("std",
				"external command print 0;" +
				"external command error 1;" +

				"external command count 0;" +
				"external command append 1;" +
				"external command at 1;" +
				"external command take 1;" +
				"external command enumerate 2;" +

				""
			));

			engine.RegisterSingletonCommand("print", new PrintCommand());
			engine.RegisterSingletonCommand("error", new ErrorCommand());

			engine.RegisterSingletonCommand("count", new CountCommand());
			engine.RegisterSingletonCommand("append", new AppendCommand());
			engine.RegisterSingletonCommand("at", new AtCommand());
			engine.RegisterSingletonCommand("take", new TakeCommand());
			engine.RegisterSingletonCommand("enumerate", new EnumerateCommand());
		}
	}
}
