using Xunit;
using Maestro;

public sealed class IfTests
{
	[Theory]
	[InlineData("if true {assert;}")]
	[InlineData("if 1 {assert;}")]
	[InlineData("if 4.3 {assert;}")]
	[InlineData("if \"string\" {assert;}")]
	[InlineData("if false {} else {assert;}")]
	[InlineData("if 0 {} else {assert;}")]
	[InlineData("if 0.0 {} else {assert;}")]
	[InlineData("if \"\" {} else {assert;}")]
	[InlineData("if false {} else if true {assert;}")]
	[InlineData("if 0 {} else if 1 {assert;}")]
	[InlineData("if 0.0 {} else if 0.5 {assert;}")]
	[InlineData("if \"\" {} else if \"string\" {assert;}")]
	[InlineData("if true, 1, \"string\" {assert;}")]
	[InlineData("if true, true, false {} else {assert;}")]
	[InlineData("if false, true, true {} else {assert;}")]
	[InlineData("if true | bypass {assert;}")]
	[InlineData("if true, 1, \"string\" | bypass {assert;}")]
	public void IfTrue(string source)
	{
		var assertCommand = new AssertCommand(new object[0]);

		var engine = new Engine();
		engine.RegisterCommand("assert", () => assertCommand);
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		source = "native command assert 0;native command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
		assertCommand.AssertExpectedInputs();
	}

	[Theory]
	[InlineData("if false {assert;}")]
	[InlineData("if 0 {assert;}")]
	[InlineData("if 0.0 {assert;}")]
	[InlineData("if \"\" {assert;}")]
	[InlineData("if true {} else {assert;}")]
	[InlineData("if 1 {} else {assert;}")]
	[InlineData("if 4.3 {} else {assert;}")]
	[InlineData("if \"string\" {} else {assert;}")]
	[InlineData("if true, true, false {assert;}")]
	[InlineData("if false, true, true {assert;}")]
	public void IfFalse(string source)
	{
		var assertCommand = new AssertCommand(new object[0]);

		var engine = new Engine();
		engine.RegisterCommand("assert", () => assertCommand);
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		source = "native command assert 0;native command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
		assertCommand.AssertNotCalled();
	}
}