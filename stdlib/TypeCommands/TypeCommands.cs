namespace Maestro.StdLib
{
	public static class EngineTypeCommandsExtensions
	{
		public static void RegisterTypeCommands(this Engine engine)
		{
			engine.RegisterLibrary(new Source("types",
				"external command is-null 0;" +
				"external command is-bool 0;" +
				"external command is-int 0;" +
				"external command is-float 0;" +
				"external command is-string 0;" +
				"external command is-object 0;" +

				"external command only-nulls 0;" +
				"external command only-bools 0;" +
				"external command only-ints 0;" +
				"external command only-floats 0;" +
				"external command only-strings 0;" +
				"external command only-objects 0;" +

				"external command except-nulls 0;" +
				"external command except-bools 0;" +
				"external command except-ints 0;" +
				"external command except-floats 0;" +
				"external command except-strings 0;" +
				"external command except-objects 0;" +

				""
			));

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
