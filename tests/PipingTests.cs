using Xunit;
using Maestro;

public sealed class PipingTests
{
	[Theory]
	[InlineData("1 | assert;", 1)]
	[InlineData("1 | bypass | assert;", 1)]
	[InlineData("1, 2 | assert;", 1, 2)]
	[InlineData("1, 2, (3 | bypass) | assert;", 1, 2, 3)]
	public void SimplePiping(string source, params int[] expected)
	{
		var expectedValues = new Value[expected.Length];
		for (var i = 0; i < expectedValues.Length; i++)
			expectedValues[i] = new Value(expected[i]);
		var assertCommand = new AssertCommand(expectedValues);

		var engine = new Engine();
		engine.RegisterCommand("assert", () => assertCommand);
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		source = "external command assert 0;external command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
		assertCommand.AssertExpectedInputs();
	}
}