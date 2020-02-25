using Xunit;
using Maestro;

public sealed class ParserTests
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
	[InlineData("(false);")]
	[InlineData("(99, true, \"string\");")]
	[InlineData("(((false)));")]
	public void SimpleStatement(string source)
	{
		TestHelper.Compile(new Engine(), source);
	}

	[Theory]
	[InlineData("bypass 0;")]
	[InlineData("1, false | bypass 0;")]
	[InlineData("1, false | bypass 0 | bypass 1;")]
	[InlineData("1, false | bypass (\"string\" | bypass 1);")]
	[InlineData("1, false | bypass bypass bypass 0;")]
	public void ExecuteCommandStatement(string source)
	{
		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple1>());
		TestHelper.Compile(engine, source).Run();
	}

	[Theory]
	[InlineData("0")]
	[InlineData("1 false | bypass 0;")]
	[InlineData("(0;")]
	[InlineData("0);")]
	[InlineData(")0;")]
	[InlineData("(0;);")]
	public void FailStatement(string source)
	{
		Assert.Throws<CompileErrorException>(() => {
			var engine = new Engine();
			engine.RegisterCommand("bypass", () => new BypassCommand<Tuple1>());
			TestHelper.Compile(engine, source);
		});
	}
}