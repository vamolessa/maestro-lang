using Xunit;
using Maestro;

public sealed class MultipleAssemblyTests
{
	[Theory]
	[InlineData("", "")]
	[InlineData("export command c {}", "c;")]
	public void ImportCommand(string libSource, string source)
	{
		var engine = new Engine();
		var libResult = engine.CompileSource(new Source("lib", libSource), TestHelper.CompileMode);
		Assert.Equal(0, libResult.errors.count);
		TestHelper.Compile(engine, source).Run();
	}

	[Theory]
	[InlineData("", "c;")]
	[InlineData("command c {}", "c;")]
	public void ImportFail(string libSource, string source)
	{
		Assert.Throws<CompileErrorException>(() => {
			var engine = new Engine();
			var libResult = engine.CompileSource(new Source("lib", libSource), TestHelper.CompileMode);
			Assert.Equal(0, libResult.errors.count);
			TestHelper.Compile(engine, source).Run();
		});
	}
}