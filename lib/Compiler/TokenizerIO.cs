namespace Rain
{
	internal sealed class TokenizerIO
	{
		public string source;
		public int nextIndex;

		public TokenizerIO()
		{
			Reset(string.Empty, 0);
		}

		public void Reset(string source, int nextIndex)
		{
			this.source = source;
			this.nextIndex = nextIndex;
		}

		public Token MakeToken(TokenKind tokenKind, int index)
		{
			return new Token(tokenKind, new Slice(index, nextIndex - index));
		}

		public void IgnoreChars(string ignored)
		{
			while (ignored.IndexOf(Peek()) >= 0)
				NextChar();
		}

		public bool IsAtEnd()
		{
			return nextIndex >= source.Length;
		}

		public char NextChar()
		{
			return source[nextIndex++];
		}

		public char Peek()
		{
			return source[nextIndex];
		}
	}
}