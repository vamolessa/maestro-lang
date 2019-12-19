namespace Rain
{
	internal sealed class Tokenizer
	{
		public readonly TokenizerIO io = new TokenizerIO();
		private Option<Token> pendingToken = Option.None;

		public void Reset(string source, int nextIndex)
		{
			io.Reset(source, nextIndex);
		}

		public Token Next()
		{
			if (pendingToken.isSome)
			{
				var token = pendingToken.value;
				pendingToken = Option.None;
				return token;
			}

			io.IgnoreChars(" \r");

			if (io.nextIndex == io.source.Length)
			{
				io.NextChar();
				return new Token(TokenKind.NewLine, new Slice(io.source.Length, 0));
			}
			else if (io.nextIndex > io.source.Length)
			{
				return new Token(TokenKind.End, new Slice(io.source.Length, 0));
			}

			while (!io.IsAtEnd())
			{
				var startIndex = io.nextIndex;
				var ch = io.NextChar();
				switch (ch)
				{
				case '\n':
					return io.MakeToken(TokenKind.NewLine, startIndex);
				case '#':
					while (io.Peek() != '\n')
						io.NextChar();
					break;
				default:
					break;
				}
			}

			return io.MakeToken(TokenKind.Error, startIndex);
		}
	}
}