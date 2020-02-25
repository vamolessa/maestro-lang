using Xunit;
using Maestro;

public sealed class ReturnTests
{
	[Theory]
	[InlineData("command c {called; return; not-called;} c;")]
	[InlineData("command c {if true {called; return;} not-called;} c;")]
	[InlineData("command c {if true {called; return; not-called;}} c;")]
	[InlineData("command c {foreach $_ in 1, 2 {called; return;} not-called;} c;")]
	[InlineData("command c {foreach $_ in 1, 2 {called; return; not-called;}} c;")]
	public void Flow(string source)
	{
		var assertCalledCommand = new AssertCommand();
		var assertNotCalledCommand = new AssertCommand();

		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		engine.RegisterCommand("called", () => assertCalledCommand);
		engine.RegisterCommand("not-called", () => assertNotCalledCommand);
		source = "native command bypass 0;native command called 0;native command not-called 0;\n" + source;
		TestHelper.Compile(engine, source).Run();

		assertCalledCommand.AssertExpectedInputs();
		assertNotCalledCommand.AssertNotCalled();
	}

	[Theory]
	[InlineData("return;")]
	[InlineData("return 1;", 1)]
	[InlineData("return 1, 2, 3;", 1, 2, 3)]
	[InlineData("return 1, true, \"string\";", 1, true, "string")]
	public void ReturnFromRoot(string source, params object[] expected)
	{
		var engine = new Engine();
		var compiled = TestHelper.Compile(engine, source);
		using (var s = compiled.ExecuteScope())
		{
			s.Run(default, expected.Length);

			var returns = new Value[s.scope.StackCount];
			for (var i = 0; i < returns.Length; i++)
				returns[i] = s.scope[i];

			Assert.Equal(expected, TestHelper.ToObjectArray(returns));
		}
	}
}