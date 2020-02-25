using Xunit;
using Maestro;
using Maestro.StdLib;

public sealed class TypeCommandTests
{
	private sealed class NullCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			context.PushValue(default);
		}
	}

	private sealed class ObjectCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			context.PushValue(new Value(new object()));
		}
	}

	[Theory]
	[InlineData("is-null | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | is-null | assert;", true, false, false, false, false, false, false)]
	[InlineData("is-bool | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | is-bool | assert;", false, true, true, false, false, false, false)]
	[InlineData("is-int | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | is-int | assert;", false, false, false, true, false, false, false)]
	[InlineData("is-float | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | is-float | assert;", false, false, false, false, true, false, false)]
	[InlineData("is-string | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | is-string | assert;", false, false, false, false, false, true, false)]
	[InlineData("is-object | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | is-object | assert;", false, false, false, false, false, false, true)]

	[InlineData("only-nulls | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | only-nulls | assert;", null)]
	[InlineData("only-bools | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | only-bools | assert;", false, true)]
	[InlineData("only-ints | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | only-ints | assert;", 0)]
	[InlineData("only-floats | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | only-floats | assert;", 0.5f)]
	[InlineData("only-strings | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\", object | only-strings | assert;", "string")]
	[InlineData("only-objects | assert;")]

	[InlineData("except-nulls | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\" | except-nulls | assert;", false, true, 0, 0.5f, "string")]
	[InlineData("except-bools | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\" | except-bools | assert;", null, 0, 0.5f, "string")]
	[InlineData("except-ints | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\" | except-ints | assert;", null, false, true, 0.5f, "string")]
	[InlineData("except-floats | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\" | except-floats | assert;", null, false, true, 0, "string")]
	[InlineData("except-strings | assert;")]
	[InlineData("null, false, true, 0, 0.5, \"string\" | except-strings | assert;", null, false, true, 0, 0.5f)]
	[InlineData("except-objects | assert;")]
	public void Tests(string source, params object[] expected)
	{
		expected = expected ?? new object[] { null };
		var assertCommand = new AssertCommand(expected);

		var engine = new Engine();
		engine.RegisterTypeCommands();
		engine.RegisterCommand("assert", () => assertCommand);
		engine.RegisterSingletonCommand("null", new NullCommand());
		engine.RegisterSingletonCommand("object", new ObjectCommand());
		TestHelper.Compile(engine, source).Run();

		assertCommand.AssertExpectedInputs();
	}
}