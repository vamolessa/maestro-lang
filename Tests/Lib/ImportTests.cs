using Xunit;
using Maestro;

public sealed class ImportTests
{
	[Theory]
	[InlineData("", "")]
	[InlineData("command c {}", "c;")]
	public void ImportLibrary(string libSource, string source)
	{
		var engine = new Engine();
		engine.RegisterLibrary(new Source("lib", libSource));

		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		libSource = "native command bypass 0;\n" + libSource;
		source = "native command bypass 0;import \"lib\";\n" + source;
		TestHelper.Compile(engine, source).Run();
	}

	[Theory]
	[InlineData("", "import \"lib\";")]
	public void ImportFail(string libSource, string source)
	{
		Assert.Throws<CompileErrorException>(() => {
			var engine = new Engine();

			engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
			libSource = "native command bypass 0;\n" + libSource;
			source = "native command bypass 0;\n" + source;
			TestHelper.Compile(engine, source).Run();
		});
	}
}