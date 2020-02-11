using Xunit;
using Maestro;

public sealed class VariableTests
{
	[Theory]
	[InlineData("1 | $var; $var | bypass;")]
	[InlineData("9.4 | $var; $var | bypass;")]
	[InlineData("true | $var; $var | bypass;")]
	[InlineData("false | $var; $var | bypass;")]
	[InlineData("\"string\" | $var; $var | bypass;")]
	[InlineData("bypass | $var; $var | bypass;")]
	[InlineData("1, 3.5, true, false, \"string\", bypass | $var; $var | bypass;")]
	[InlineData("1, 0.5 | $var0, $var1; $var0, $var1 | bypass;")]
	[InlineData("0.5, true | $var0, $var1; $var0, $var1 | bypass;")]
	[InlineData("true, false | $var0, $var1; $var0, $var1 | bypass;")]
	[InlineData("false, \"string\" | $var0, $var1; $var0, $var1 | bypass;")]
	[InlineData("\"string\", bypass | $var0, $var1; $var0, $var1 | bypass;")]
	[InlineData("bypass | $var0, $var1; $var0, $var1 | bypass;")]
	[InlineData("1 | $var0, $var1; $var0, $var1 | bypass;")]
	[InlineData("1, 3.5, true, false, \"string\", bypass | $var0, $var1; $var0, $var1 | bypass;")]
	public void Declaration(string source)
	{
		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		source = "external command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
	}

	[Theory]
	[InlineData("$var;")]
	[InlineData("1 | $var;")]
	[InlineData("$var | bypass;")]
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

	[Theory]
	[InlineData("bypass | $var; $var | assert;", null)]
	[InlineData("1 | $var; $var | assert;", 1)]
	[InlineData("1, 2 | $var; $var | assert;", 1)]
	[InlineData("1 | $var0, $var1; $var0, $var1 | assert;", 1, null)]
	[InlineData("1, 2 | $var0, $var1; $var0, $var1 | assert;", 1, 2)]
	[InlineData("1, 2, 3 | $var0, $var1; $var0, $var1 | assert;", 1, 2)]
	[InlineData("33 | $var0; $var0 | $var1; $var1 | $var2; $var2 | assert;", 33)]
	public void Values(string source, params object[] expected)
	{
		expected = expected ?? new object[] { null };
		var assertCommand = new AssertCommand(expected);

		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		engine.RegisterCommand("assert", () => assertCommand);
		source = "external command assert 0;external command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();

		assertCommand.AssertExpectedInputs();
	}
}