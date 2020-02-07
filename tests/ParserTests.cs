using Xunit;

public sealed class ParserTest
{
	[Theory]
	[InlineData("0;")]
	public void TestExpressions(string source)
	{
		TestHelper.Compile(source);
	}
}