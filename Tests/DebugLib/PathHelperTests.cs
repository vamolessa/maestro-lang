using Xunit;
using Maestro.Debug;

public sealed class PathHelperTests
{
	[Theory]
	[InlineData("", 0)]
	[InlineData("path", 4)]
	[InlineData("some/path", 9)]
	[InlineData("some\\path", 9)]
	[InlineData(".extension", 0)]
	[InlineData("path.extension", 4)]
	[InlineData("some/path.extension", 9)]
	[InlineData("some\\path.extension", 9)]
	[InlineData("some.not_extension/path", 23)]
	[InlineData("some.not_extension\\path", 23)]
	public void GetLengthWithoutExtension(string path, int expected)
	{
		Assert.Equal(expected, PathHelper.GetLengthWithoutExtension(path));
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("/", "/")]
	[InlineData("\\", "\\")]
	[InlineData("/", "\\")]
	[InlineData("\\", "/")]
	[InlineData("path.ext", "path.extension")]
	[InlineData("path.extension", "path.ext")]
	[InlineData("some/path.ext", "some/path.extension")]
	[InlineData("some/path.extension", "some/path.ext")]
	[InlineData("some\\path.extension", "some/path.ext")]
	[InlineData("some/path.extension", "some\\path.ext")]
	[InlineData("some\\path.ext", "some/path.extension")]
	[InlineData("some/path.ext", "some\\path.extension")]
	public void EndsWith(string path, string match)
	{
		Assert.True(PathHelper.EndsWith(path, match));
	}
}