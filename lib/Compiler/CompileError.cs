namespace Rain
{
	public enum CompileErrorType
	{
		InvalidToken,
		ExpectedNodeName,
		ExpectedNewLine,
	}

	public interface ICompileErrorContext
	{
	}

	public readonly struct CompileError
	{
		public readonly int sourceIndex;
		public readonly Slice slice;
		public readonly CompileErrorType type;
		public readonly ICompileErrorContext context;

		public CompileError(int sourceIndex, Slice slice, CompileErrorType type, ICompileErrorContext context)
		{
			this.sourceIndex = sourceIndex;
			this.slice = slice;
			this.type = type;
			this.context = context;
		}
	}
}