namespace Flow
{
	public interface ICompileErrorMessage
	{
		string Message();
	}

	public readonly struct CompileError
	{
		public readonly int sourceIndex;
		public readonly Slice slice;
		public readonly ICompileErrorMessage message;

		public CompileError(int sourceIndex, Slice slice, ICompileErrorMessage message)
		{
			this.sourceIndex = sourceIndex;
			this.slice = slice;
			this.message = message;
		}
	}
}