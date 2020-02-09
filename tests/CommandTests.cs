using Xunit;
using Maestro;

public sealed class CommandTests
{
	[Theory]
	[InlineData("command c {}")]
	[InlineData("command c $_arg1 {}")]
	[InlineData("command c $_arg1 $_arg2 {}")]
	[InlineData("command c $_arg1 $_arg2 $_arg3 {}")]
	public void Declaration(string source)
	{
		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		source = "external command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
	}

	[Theory]
	[InlineData("command c $arg {}")]
	[InlineData("command c $_arg $_arg {}")]
	public void FailDeclaration(string source)
	{
		Assert.Throws<CompileErrorException>(() =>
		{
			var engine = new Engine();
			engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
			source = "external command bypass 0;\n" + source;
			TestHelper.Compile(engine, source).Run();
		});
	}
}