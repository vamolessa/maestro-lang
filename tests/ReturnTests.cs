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
		source = "external command bypass 0;external command called 0;external command not-called 0;\n" + source;
		TestHelper.Compile(engine, source).Run();

		assertCalledCommand.AssertExpectedInputs();
		assertNotCalledCommand.AssertNotCalled();
	}
}