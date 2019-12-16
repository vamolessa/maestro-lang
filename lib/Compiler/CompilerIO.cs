namespace Rain
{
	internal sealed class CompilerIO
	{
		public readonly Parser parser;
		public int sourceIndex;
		public bool isInPanicMode;

		public Buffer<CompileError> errors = new Buffer<CompileError>();

		public CompilerIO()
		{
			void AddTokenizerError(Slice slice, CompileErrorType errorType, ICompileErrorContext context)
			{
				AddHardError(slice, errorType, context);
			}

			var tokenizer = new Tokenizer(TokenScanners.scanners);
			parser = new Parser(tokenizer, AddTokenizerError);
		}

		public void Reset()
		{
			errors.count = 0;
		}

		public void AddSoftError(Slice slice, CompileErrorType errorType, ICompileErrorContext context)
		{
			if (!isInPanicMode)
				errors.PushBack(new CompileError(sourceIndex, slice, errorType, context));
		}

		public void AddHardError(Slice slice, CompileErrorType errorType, ICompileErrorContext context)
		{
			if (!isInPanicMode)
				errors.PushBack(new CompileError(sourceIndex, slice, errorType, context));
		}
	}
}