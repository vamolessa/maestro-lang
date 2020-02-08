using Xunit;
using Maestro;

public sealed class ParserTest
{
	[Theory]
	[InlineData("0;")]
	[InlineData("+0;")]
	[InlineData("-0;")]
	[InlineData("1.05;")]
	[InlineData("-1.05;")]
	[InlineData("true;")]
	[InlineData("false;")]
	[InlineData("\"string\";")]
	[InlineData("99, true, \"string\";")]
	public void TestSimpleExpressions(string source)
	{
		TestHelper.Compile(source);
	}

	[Theory]
	[InlineData("bypass 0;")]
	[InlineData("1, false | bypass 0")]
	[InlineData("1, false | bypass 0 | bypass 1;")]
	[InlineData("1, false | bypass (\"string\" | bypass 1)")]
	public void TestCommandExpressions(string source)
	{
		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple1>());
		source = "extern command bypass 1;\n" + source;
		TestHelper.Compile(engine, source);
	}
}