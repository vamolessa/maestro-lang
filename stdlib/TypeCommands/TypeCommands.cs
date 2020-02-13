namespace Maestro.StdLib
{
	public static class EngineTypeCommandsExtensions
	{
		public static void RegisterTypeCommands(this Engine engine)
		{
			engine.RegisterLibrary(new Source("types",
				"external command is-null 1;" +
				"external command is-bool 1;" +
				"external command is-int 1;" +
				"external command is-float 1;" +
				"external command is-string 1;" +
				"external command is-object 1;" +

				"external command keep-nulls 0;" +
				"external command keep-bools 0;" +
				"external command keep-ints 0;" +
				"external command keep-floats 0;" +
				"external command keep-strings 0;" +
				"external command keep-objects 0;" +

				"external command remove-nulls 0;" +
				"external command remove-bools 0;" +
				"external command remove-ints 0;" +
				"external command remove-floats 0;" +
				"external command remove-strings 0;" +
				"external command remove-objects 0;" +

				""
			));

			engine.RegisterSingletonCommand("is-null", new IsNullCommand());
			engine.RegisterSingletonCommand("is-bool", new IsBoolCommand());
			engine.RegisterSingletonCommand("is-int", new IsIntCommand());
			engine.RegisterSingletonCommand("is-float", new IsFloatCommand());
			engine.RegisterSingletonCommand("is-string", new IsStringCommand());
			engine.RegisterSingletonCommand("is-object", new IsObjectCommand());

			engine.RegisterSingletonCommand("keep-nulls", new KeepNullsCommand());
			engine.RegisterSingletonCommand("keep-bools", new KeepBoolsCommand());
			engine.RegisterSingletonCommand("keep-ints", new KeepIntsCommand());
			engine.RegisterSingletonCommand("keep-floats", new KeepFloatsCommand());
			engine.RegisterSingletonCommand("keep-strings", new KeepStringsCommand());
			engine.RegisterSingletonCommand("keep-objects", new KeepObjectsCommand());

			engine.RegisterSingletonCommand("remove-nulls", new RemoveNullsCommand());
			engine.RegisterSingletonCommand("remove-bools", new RemoveBoolsCommand());
			engine.RegisterSingletonCommand("remove-ints", new RemoveIntsCommand());
			engine.RegisterSingletonCommand("remove-floats", new RemoveFloatsCommand());
			engine.RegisterSingletonCommand("remove-strings", new RemoveStringsCommand());
			engine.RegisterSingletonCommand("remove-objects", new RemoveObjectsCommand());
		}
	}
}
