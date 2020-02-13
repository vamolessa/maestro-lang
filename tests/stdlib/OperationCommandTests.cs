using Xunit;
using Maestro;
using Maestro.StdLib;

public sealed class OperationCommandTests
{
	[Theory]
	[InlineData("and | assert;", true)]
	[InlineData("true | and | assert;", true)]
	[InlineData("true, 1, \"string\" | and | assert;", true)]
	[InlineData("false | and | assert;", false)]
	[InlineData("true, 1, \"string\", false | and | assert;", false)]
	[InlineData("false, true, 1, \"string\" | and | assert;", false)]

	[InlineData("or | assert;", false)]
	[InlineData("false | or | assert;", false)]
	[InlineData("false, 0, \"\" | or | assert;", false)]
	[InlineData("true | or | assert;", true)]
	[InlineData("false, 0, \"\", true | or | assert;", true)]
	[InlineData("true, false, 0, \"\" | or | assert;", true)]

	[InlineData("not | assert;")]
	[InlineData("false | not | assert;", true)]
	[InlineData("true | not | assert;", false)]
	[InlineData("0 | not | assert;", true)]
	[InlineData("1 | not | assert;", false)]
	[InlineData("\"\" | not | assert;", true)]
	[InlineData("\"string\" | not | assert;", false)]
	[InlineData("false, 0, \"\" | not | assert;", true, true, true)]
	[InlineData("true, 1, \"string\" | not | assert;", false, false, false)]
	[InlineData("true, false, 0, \"\" | not | assert;", false, true, true, true)]
	[InlineData("false, true, 1, \"string\" | not | assert;", true, false, false, false)]

	[InlineData("0 | + +1 | assert;", 1)]
	[InlineData("0 | + -1 | assert;", -1)]
	[InlineData("0, 1, 2.5 | + 1 | assert;", 1, 2, 3.5f)]
	[InlineData("0 | - +1 | assert;", -1)]
	[InlineData("0 | - -1 | assert;", 1)]
	[InlineData("0, 1, 2.5 | - 1 | assert;", -1, 0, 1.5f)]
	[InlineData("1 | * +2 | assert;", 2)]
	[InlineData("1 | * -2 | assert;", -2)]
	[InlineData("0, 1, 2.5 | * 2 | assert;", 0, 2, 5.0f)]
	[InlineData("8 | / +2 | assert;", 4)]
	[InlineData("8 | / -2 | assert;", -4)]
	[InlineData("0, 8, 7, 5.0 | / 2 | assert;", 0, 4, 3, 2.5f)]

	[InlineData("0 | < 1 | assert;", true)]
	[InlineData("2 | < 1 | assert;", false)]
	[InlineData("0, 1, 2, 0.5, 1.5 | < 1 | assert;", true, false, false, true, false)]
	[InlineData("0 | > 1 | assert;", false)]
	[InlineData("2 | > 1 | assert;", true)]
	[InlineData("0, 1, 2, 0.5, 1.5 | > 1 | assert;", false, false, true, false, true)]

	[InlineData("true | = true | assert;", true)]
	[InlineData("false | = false | assert;", true)]
	[InlineData("true | = false | assert;", false)]
	[InlineData("false | = true | assert;", false)]
	[InlineData("1 | = 1 | assert;", true)]
	[InlineData("0, 0.5, 1, 1.5, 2 | = 1 | assert;", false, false, true, false, false)]
	[InlineData("1.0 | = 1 | assert;", true)]
	[InlineData("\"\" | = \"\" | assert;", true)]
	[InlineData("\"\" | = \"string\" | assert;", false)]
	public void Tests(string source, params object[] expected)
	{
		var assertCommand = new AssertCommand(expected);

		var engine = new Engine();
		engine.RegisterOperationCommands();
		engine.RegisterCommand("assert", () => assertCommand);
		source = "import \"ops\";external command assert 0;\n" + source;
		TestHelper.Compile(engine, source).Run();

		assertCommand.AssertExpectedInputs();
	}
}