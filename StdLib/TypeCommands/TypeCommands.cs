namespace Maestro.StdLib
{
	public static class EngineTypeCommandsExtensions
	{
		public static void RegisterTypeCommands(this Engine engine)
		{
			engine.RegisterSingletonCommand("is-null", new IsNullCommand());
			engine.RegisterSingletonCommand("is-bool", new IsBoolCommand());
			engine.RegisterSingletonCommand("is-int", new IsIntCommand());
			engine.RegisterSingletonCommand("is-float", new IsFloatCommand());
			engine.RegisterSingletonCommand("is-string", new IsStringCommand());
			engine.RegisterSingletonCommand("is-object", new IsObjectCommand());

			engine.RegisterSingletonCommand("only-nulls", new OnlyNullsCommand());
			engine.RegisterSingletonCommand("only-bools", new OnlyBoolsCommand());
			engine.RegisterSingletonCommand("only-ints", new OnlyIntsCommand());
			engine.RegisterSingletonCommand("only-floats", new OnlyFloatsCommand());
			engine.RegisterSingletonCommand("only-strings", new OnlyStringsCommand());
			engine.RegisterSingletonCommand("only-objects", new OnlyObjectsCommand());

			engine.RegisterSingletonCommand("except-nulls", new ExceptNullsCommand());
			engine.RegisterSingletonCommand("except-bools", new ExceptBoolsCommand());
			engine.RegisterSingletonCommand("except-ints", new ExceptIntsCommand());
			engine.RegisterSingletonCommand("except-floats", new ExceptFloatsCommand());
			engine.RegisterSingletonCommand("except-strings", new ExceptStringsCommand());
			engine.RegisterSingletonCommand("except-objects", new ExceptObjectsCommand());
		}
	}
}
