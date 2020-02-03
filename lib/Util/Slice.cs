namespace Flow
{
	public readonly struct Slice
	{
		public readonly ushort index;
		public readonly ushort length;

		public Slice(ushort index, ushort length)
		{
			this.index = index;
			this.length = length;
		}

		public Slice(int index, int length)
		{
			this.index = (ushort)index;
			this.length = (ushort)length;
		}

		public Slice ExpandedTo(Slice other)
		{
			return new Slice(index, other.index + other.length - index);
		}
	}
}