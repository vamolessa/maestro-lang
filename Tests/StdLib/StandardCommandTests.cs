using Xunit;
using Maestro;
using Maestro.StdLib;

public sealed class StandardCommandTests
{
	[Theory]
	[InlineData("count | assert;", 0)]
	[InlineData("0 | count | assert;", 1)]
	[InlineData("0, 1, false | count | assert;", 3)]

	[InlineData("append 7 | assert;", 7)]
	[InlineData("1 | append 7 | assert;", 1, 7)]
	[InlineData("1, 2, 3 | append 7 | assert;", 1, 2, 3, 7)]

	[InlineData("at 0 | assert;", null)]
	[InlineData("7 | at 0 | assert;", 7)]
	[InlineData("7 | at 1 | assert;", null)]
	[InlineData("7, 8, 9 | at 0 | assert;", 7)]
	[InlineData("7, 8, 9 | at 1 | assert;", 8)]
	[InlineData("7, 8, 9 | at 2 | assert;", 9)]
	[InlineData("7, 8, 9 | at 3 | assert;", null)]
	[InlineData("7, 8, 9 | at 999 | assert;", null)]
	[InlineData("7, 8, 9 | at -1 | assert;", null)]

	[InlineData("take 0 | assert;")]
	[InlineData("take 3 | assert;", null, null, null)]
	[InlineData("7 | take 0 | assert;")]
	[InlineData("7 | take 1 | assert;", 7)]
	[InlineData("7 | take 3 | assert;", 7, null, null)]
	[InlineData("7, 8, 9 | take 0 | assert;")]
	[InlineData("7, 8, 9 | take 1 | assert;", 7)]
	[InlineData("7, 8, 9 | take 2 | assert;", 7, 8)]
	[InlineData("7, 8, 9 | take 3 | assert;", 7, 8, 9)]
	[InlineData("7, 8, 9 | take 4 | assert;", 7, 8, 9, null)]
	[InlineData("7, 8, 9 | take -1 | assert;")]

	[InlineData("enumerate 0 0 | assert;")]
	[InlineData("enumerate 99 0 | assert;")]
	[InlineData("enumerate -43 0 | assert;")]
	[InlineData("enumerate 0 3 | assert;", 0, 1, 2)]
	[InlineData("enumerate 5 3 | assert;", 5, 6, 7)]
	[InlineData("enumerate -1 3 | assert;", -1, 0, 1)]
	public void Tests(string source, params object[] expected)
	{
		expected = expected ?? new object[] { null };
		var assertCommand = new AssertCommand(expected);

		var engine = new Engine();
		engine.RegisterStandardCommands(t => { });
		engine.RegisterCommand("assert", () => assertCommand);
		source = "import \"std\";native command assert 0;\n" + source;
		TestHelper.Compile(engine, source).Run();

		assertCommand.AssertExpectedInputs();
	}

	[Theory]
	[InlineData("error 0;")]
	[InlineData("error false;")]
	[InlineData("error \"this is an error!\";")]
	public void Error(string source)
	{
		Assert.Throws<RuntimeErrorException>(() => {
			var engine = new Engine();
			engine.RegisterStandardCommands(t => { });
			source = "import \"std\";\n" + source;
			TestHelper.Compile(engine, source).Run(1);
		});
	}
}