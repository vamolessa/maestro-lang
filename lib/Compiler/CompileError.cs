namespace Flow
{
	public interface IFormattedMessage
	{
		string Format();
	}

	public readonly struct CompileError
	{
		public readonly int sourceIndex;
		public readonly Slice slice;
		public readonly IFormattedMessage message;

		public CompileError(int sourceIndex, Slice slice, IFormattedMessage message)
		{
			this.sourceIndex = sourceIndex;
			this.slice = slice;
			this.message = message;
		}
	}
}