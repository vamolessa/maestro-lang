namespace Rain
{
	internal sealed class Tokenizer
	{
		public readonly TokenizerIO io = new TokenizerIO();
		private readonly Scanner[] scanners;

		public Tokenizer(Scanner[] scanners)
		{
			this.scanners = scanners;
			Reset(string.Empty, 0);
		}

		public void Reset(string source, int nextIndex)
		{

		}

		public Token Next()
		{
			while (!io.IsAtEnd())
			{
				var tokenLength = 0;
				var tokenKind = TokenKind.Error;
				var scanState = default(TokenizerIO.State);
				var savedState = io.state;

				foreach (var scanner in scanners)
				{
					io.state = savedState;
					scanner.Scan(io);

					var length = io.state.nextIndex - savedState.nextIndex;
					if (tokenLength >= length)
						continue;

					tokenLength = length;
					tokenKind = scanner.tokenKind;
					scanState = io.state;
				}

				if (tokenLength == 0)
					tokenLength = 1;

				var token = new Token(tokenKind, new Slice(savedState.nextIndex, tokenLength));
				return token;
			}

			return new Token(TokenKind.End, new Slice(io.source.Length, 0));
		}
	}
}