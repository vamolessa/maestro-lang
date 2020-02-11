using Xunit;
using Maestro;

public sealed class ImportTests
{
	public sealed class TestImportResolver : IImportResolver
	{
		public readonly string otherSource;

		public TestImportResolver(string otherSource)
		{
			this.otherSource = otherSource;
		}

		public Option<string> ResolveSource(Uri requestingSourceUri, Uri importUri)
		{
			return otherSource;
		}
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("command c {}", "c;")]
	public void TwoSources(string otherSource, string source)
	{
		var importResolver = new TestImportResolver(otherSource);

		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		otherSource = "external command bypass 0;\n" + otherSource;
		source = "external command bypass 0;import \"otherSource\";\n" + source;
		TestHelper.Compile(engine, source, importResolver).Run();
	}
}