namespace Flow
{
	internal sealed class CompilerIO
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
		public int sourceIndex;
		public bool isInPanicMode;

		private Buffer<StateFrame> stateFrameStack = new Buffer<StateFrame>();
		public Buffer<CompileError> errors = new Buffer<CompileError>();

		public CompilerIO()
		{
			void AddTokenizerError(Slice slice, IFormattedMessage error)
			{
				AddHardError(slice, error);
			}

			parser = new Parser(AddTokenizerError);
		}

		public void Reset()
		{
			errors.count = 0;
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