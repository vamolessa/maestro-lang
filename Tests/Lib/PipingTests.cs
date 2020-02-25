using Xunit;
using Maestro;

public sealed class PipingTests
{
	[Theory]
	[InlineData("1 | assert;", 1)]
	[InlineData("1 | bypass | assert;", 1)]
	[InlineData("1, 2 | assert;", 1, 2)]
	[InlineData("1, 2, (3 | bypass) | assert;", 1, 2, 3)]
	public void SimplePiping(string source, params object[] expected)
	{
		var assertCommand = new AssertCommand(expected);

		var engine = new Engine();
		engine.RegisterCommand("assert", () => assertCommand);
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		TestHelper.Compile(engine, source).Run();
		assertCommand.AssertExpectedInputs();
	}

	[Theory]
	[InlineData("$$ | assert;")]
	[InlineData("$$ | assert;", 1)]
	[InlineData("$$ | assert;", 1, 2, 3)]
	[InlineData("$$ | assert;", 1, true, null, "string")]
	public void Input(string source, params object[] expected)
	{
		var values = TestHelper.ToValueArray(expected);
		var assertCommand = new AssertCommand(expected);

		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		engine.RegisterCommand("assert", () => assertCommand);
		var compiled = TestHelper.Compile(engine, source);

		using (var s = compiled.ExecuteScope())
		{
			foreach (var value in values)
				s.scope.PushValue(value);
			s.Run(default);
		}

		assertCommand.AssertExpectedInputs();
	}
}