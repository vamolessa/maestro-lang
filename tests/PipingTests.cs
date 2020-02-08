using Xunit;
using Maestro;

public sealed class PipingTests
{
	[Theory]
	[InlineData("1 | assert;", 1)]
	[InlineData("1 | bypass | assert;", 1)]
	public void SinglePiping(string source, int expected)
	{
		var engine = new Engine();
		var assertCommand = new AssertCommand(new Value(expected));
		engine.RegisterCommand("assert", () => assertCommand);
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		source = "extern command assert 0;extern command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
		assertCommand.AssertExpectedInputs();
	}
}