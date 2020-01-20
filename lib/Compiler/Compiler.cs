namespace Flow
{
	internal sealed class Compiler
	{
		private readonly struct StateFrame
		{
			public readonly string sourceContent;
			public readonly int sourceIndex;

			public readonly int tokenizerIndex;
			public readonly Token previousToken;
			public readonly Token currentToken;

			public StateFrame(string sourceContent, int sourceIndex, int tokenizerIndex, Token previousToken, Token currentToken)
			{
				this.sourceContent = sourceContent;
				this.sourceIndex = sourceIndex;

				this.tokenizerIndex = tokenizerIndex;
				this.previousToken = previousToken;
				this.currentToken = currentToken;
			}
		}

		public readonly Parser parser;
		public ByteCodeChunk chunk;

		public int sourceIndex;
		public bool isInPanicMode;

		public Buffer<CompileError> errors = new Buffer<CompileError>();

		public Buffer<LocalVariable> localVariables = new Buffer<LocalVariable>(256);

		private Buffer<StateFrame> stateFrameStack = new Buffer<StateFrame>();

		public Compiler()
		{
			void AddTokenizerError(Slice slice, IFormattedMessage error)
			{
				AddHardError(slice, error);
			}

			parser = new Parser(AddTokenizerError);
		}

		public void Reset(ByteCodeChunk chunk)
		{
			this.chunk = chunk;
			errors.count = 0;
			stateFrameStack.count = 0;
		}

		private void RestoreState(StateFrame state)
		{
			parser.tokenizer.Reset(state.sourceContent, state.tokenizerIndex);
			parser.Reset(state.previousToken, state.currentToken);
			sourceIndex = state.sourceIndex;

			isInPanicMode = false;
		}

		public void BeginSource(string source, int sourceIndex)
		{
			stateFrameStack.PushBack(new StateFrame(
				parser.tokenizer.source,
				this.sourceIndex,

				parser.tokenizer.nextIndex,
				parser.previousToken,
				parser.currentToken
			));

			RestoreState(new StateFrame(
				source,
				sourceIndex,
				0,
				new Token(TokenKind.End, new Slice()),
				new Token(TokenKind.End, new Slice())
			));
		}

		public void EndSource()
		{
			RestoreState(stateFrameStack.PopLast());
		}

		public void AddSoftError(Slice slice, IFormattedMessage error)
		{
			if (!isInPanicMode)
				errors.PushBack(new CompileError(sourceIndex, slice, error));
		}

		public void AddHardError(Slice slice, IFormattedMessage error)
		{
			if (!isInPanicMode)
			{
				isInPanicMode = true;
				errors.PushBack(new CompileError(sourceIndex, slice, error));
			}
		}
	}
}