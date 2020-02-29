using Xunit;
using Maestro;

public sealed class BufferTests
{
	[Fact]
	public void PushBackWhenEmpty()
	{
		var buffer = new Buffer<int>();
		Assert.Null(buffer.buffer);
		Assert.Equal(0, buffer.count);

		buffer.PushBack(23);
		Assert.NotNull(buffer.buffer);
		Assert.Equal(1, buffer.count);

		buffer.PushBack(45);
		Assert.NotNull(buffer.buffer);
		Assert.Equal(2, buffer.count);

		Assert.Equal(new[] { 23, 45 }, buffer.ToArray());
	}

	[Theory]
	[InlineData(new[] { 0, 1, 2, 3 }, 0, new[] { 3, 1, 2 })]
	[InlineData(new[] { 0, 1, 2, 3 }, 1, new[] { 0, 3, 2 })]
	[InlineData(new[] { 0, 1, 2, 3 }, 2, new[] { 0, 1, 3 })]
	[InlineData(new[] { 0, 1, 2, 3 }, 3, new[] { 0, 1, 2 })]
	public void SwapRemove(int[] array, int indexToRemove, int[] expectedArray)
	{
		var buffer = new Buffer<int>(array.Length);
		foreach (var e in array)
			buffer.PushBack(e);

		Assert.Equal(array, buffer.ToArray());
		buffer.SwapRemove(indexToRemove);
		Assert.Equal(expectedArray, buffer.ToArray());
	}
}