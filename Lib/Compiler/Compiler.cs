namespace Maestro
{
	internal sealed class Compiler
	{
		public readonly Parser parser;

		public Assembly assembly;
		public Mode mode;
		public bool isInPanicMode;

		public Buffer<CompileError> errors = new Buffer<CompileError>();
		public Buffer<Variable> variables = new Buffer<Variable>(256);
		public Buffer<Scope> scopes = new Buffer<Scope>(1);

		public Compiler()
		{
			void AddTokenizerError(Slice slice, IFormattedMessage error)
			{
				AddHardError(slice, error);
			}

			parser = new Parser(AddTokenizerError);
		}

		public void Reset(Assembly assembly, Mode mode, Source source)
		{
			this.assembly = assembly;
			this.mode = mode;
			isInPanicMode = false;

			errors.ZeroClear();
			variables.count = 0;
			scopes.count = 0;

			parser.tokenizer.Reset(source.content);
			parser.Reset();
		}

		public void AddSoftError(Slice slice, IFormattedMessage error)
		{
			if (!isInPanicMode)
				errors.PushBack(new CompileError(slice, error));
		}

		public void AddHardError(Slice slice, IFormattedMessage error)
		{
			if (!isInPanicMode)
			{
				isInPanicMode = true;
				errors.PushBack(new CompileError(slice, error));
			}
		}
	}
}