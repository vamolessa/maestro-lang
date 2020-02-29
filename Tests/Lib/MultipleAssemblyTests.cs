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
		var libCompileResult = engine.CompileSource(new Source("lib", libSource), TestHelper.CompileMode);
		Assert.True(libCompileResult.TryGetAssembly(out var libAssembly));
		var libLinkResult = engine.LinkAssembly(libAssembly);
		Assert.True(libLinkResult.TryGetExecutable(out var libExecutable));
		TestHelper.Compile(engine, source).Run();
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("export command c {}", "c;")]
	public void ImportCommandOutOfOrder(string libSource, string source)
	{
		var engine = new Engine();

		var sourceCompileResult = engine.CompileSource(new Source("source", source), TestHelper.CompileMode);
		Assert.True(sourceCompileResult.TryGetAssembly(out var sourceAssembly));
		var libCompileResult = engine.CompileSource(new Source("lib", libSource), TestHelper.CompileMode);
		Assert.True(libCompileResult.TryGetAssembly(out var libAssembly));

		var sourceLinkResult = engine.LinkAssembly(sourceAssembly);
		Assert.True(sourceLinkResult.TryGetExecutable(out var sourceExecutable));
		var libLinkResult = engine.LinkAssembly(libAssembly);
		Assert.True(libLinkResult.TryGetExecutable(out var libExecutable));

		using var scope = engine.ExecuteScope();
		TestHelper.Run(engine, scope, sourceExecutable, 0);
	}

	[Theory]
	[InlineData("", "c;")]
	[InlineData("command c {}", "c;")]
	public void ImportFail(string libSource, string source)
	{
		Assert.Throws<LinkErrorException>(() => {
			var engine = new Engine();
			var libCompileResult = engine.CompileSource(new Source("lib", libSource), TestHelper.CompileMode);
			Assert.True(libCompileResult.TryGetAssembly(out var libAssembly));
			var libLinkResult = engine.LinkAssembly(libAssembly);
			Assert.True(libLinkResult.TryGetExecutable(out var libExecutable));
			TestHelper.Compile(engine, source).Run();
		});
	}
}