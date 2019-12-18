namespace Rain
{
	public sealed class ByteCodeChunk
	{
		public Buffer<byte> bytes = new Buffer<byte>(256);
		public Buffer<Slice> sourceSlices = new Buffer<Slice>(256);
		public Buffer<int> sourceStartIndexes = new Buffer<int>();

		public void WriteByte(byte value, Slice slice)
		{
			bytes.PushBack(value);
			sourceSlices.PushBack(slice);
		}
	}
}