namespace Rain
{
	internal sealed class Tokenizer
	{
		public readonly TokenizerIO io = new TokenizerIO();
		private bool atBeginingOfLine = true;
		private int indentation = 0;
		private int pendingIndentationTokens = 0;

		public void Reset(string source, int nextIndex)
		{
			atBeginingOfLine = true;
			indentation = 0;
			pendingIndentationTokens = 0;
			io.Reset(source, nextIndex);
		}

		public Token Next()
		{
			if (pendingIndentationTokens > 0)
			{
				pendingIndentationTokens -= 1;
				return io.MakeToken(TokenKind.Indent, io.nextIndex - 1);
			}
			else if (pendingIndentationTokens < 0)
			{
				pendingIndentationTokens += 1;
				return io.MakeToken(TokenKind.Dedent, io.nextIndex - 1);
			}

			if (!atBeginingOfLine)
				io.IgnoreChars(" \r\t");

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
					atBeginingOfLine = true;
					return io.MakeToken(TokenKind.NewLine, startIndex);
				case '\t':
					if (atBeginingOfLine)
					{
						var indentCount = 0;
						while (io.Peek() == '\t')
						{
							io.NextChar();
							indentCount += 1;
						}

						io.IgnoreChars(" \r\t");
						if (io.Peek() != '\n' && indentCount != indentation)
						{
							pendingIndentationTokens = indentCount - indentation;
							indentation = indentCount;

							if (pendingIndentationTokens > 0)
							{
								pendingIndentationTokens -= 1;
								return io.MakeToken(TokenKind.Indent, io.nextIndex - 1);
							}
							else
							{
								pendingIndentationTokens += 1;
								return io.MakeToken(TokenKind.Dedent, io.nextIndex - 1);
							}
						}
					}
					break;
				case '#':
					while (io.Peek() != '\n' && !io.IsAtEnd())
						io.NextChar();
					break;
				default:
					atBeginingOfLine = false;
					break;
				}
			}

			return io.MakeToken(TokenKind.Error, io.nextIndex - 1);
		}
	}
}