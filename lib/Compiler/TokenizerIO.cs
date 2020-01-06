namespace Rain
{
	internal sealed class TokenizerIO
	{
		public string source;
		public int nextIndex;
		public int indentationLevel;
		public bool atBeginingOfLine;
		public int pendingIndentation;

		public TokenizerIO()
		{
			source = string.Empty;
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

		public Char NextChar()
		{
			var c = Peek();
			if (pendingIndentation > 0)
				pendingIndentation--;
			else if (pendingIndentation < 0)
				pendingIndentation++;
			else
				nextIndex++;
			return c;
		}

		public Char Peek()
		{
			if (pendingIndentation > 0)
				return new Char(CharKind.Indent);
			else if (pendingIndentation < 0)
				return new Char(CharKind.Dedent);

			if (atBeginingOfLine)
			{
				var indent = 0;
				while (source[nextIndex] == '\t')
				{
					indent += 1;
					nextIndex += 1;
				}
			}

			return new Char(CharKind.Char, source[nextIndex]);
		}
	}
}