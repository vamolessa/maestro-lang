namespace Flow
{
	internal sealed class Parser
	{
		public readonly Tokenizer tokenizer = new Tokenizer(TokenScanners.scanners);
		private readonly System.Action<Slice, ICompileErrorMessage> onError;

		public Token previousToken;
		public Token currentToken;

		public Parser(System.Action<Slice, ICompileErrorMessage> onError)
		{
			this.onError = onError;

			Reset(new Token(TokenKind.End, new Slice()), new Token(TokenKind.End, new Slice()));
		}

		public void Reset(Token previousToken, Token currentToken)
		{
			this.previousToken = previousToken;
			this.currentToken = currentToken;
		}

		public void Next()
		{
			previousToken = currentToken;

			while (true)
			{
				currentToken = tokenizer.Next();
				if (currentToken.kind != TokenKind.Error)
					break;

				onError(currentToken.slice, new InvalidTokenError());
			}
		}

		public bool Check(TokenKind tokenKind)
		{
			return currentToken.kind == tokenKind;
		}

		public bool Match(TokenKind tokenKind)
		{
			if (currentToken.kind != tokenKind)
				return false;

			Next();
			return true;
		}

		public void Consume<E>(TokenKind tokenKind, E error) where E : struct, ICompileErrorMessage
		{
			if (currentToken.kind == tokenKind)
				Next();
			else
				onError(currentToken.slice, error);
		}
	}
}