namespace Flow
{
	internal abstract class Scanner
	{
		public TokenKind tokenKind;

		public static bool MatchStart(TokenizerIO io, string match)
		{
			for (var i = 0; i < match.Length; i++)
			{
				if (io.IsAtEnd() || io.NextChar() != match[i])
					return false;
			}

			return true;
		}

		public abstract bool Scan(TokenizerIO io);

		public Scanner ForToken(TokenKind tokenKind)
		{
			this.tokenKind = tokenKind;
			return this;
		}

		public Scanner Ignore()
		{
			this.tokenKind = TokenKind.End;
			return this;
		}
	}

	internal sealed class WhiteSpaceScanner : Scanner
	{
		public readonly string except;

		public WhiteSpaceScanner(string except)
		{
			this.except = except;
		}

		public override bool Scan(TokenizerIO io)
		{
			while (!io.IsAtEnd())
			{
				var c = io.NextChar();
				if (!char.IsWhiteSpace(c) || except.IndexOf(c) >= 0)
					break;
			}

			return true;
		}
	}

	internal sealed class LineCommentScanner : Scanner
	{
		public readonly string prefix;

		public LineCommentScanner(string prefix)
		{
			this.prefix = prefix;
		}

		public override bool Scan(TokenizerIO io)
		{
			if (!MatchStart(io, prefix))
				return false;

			while (!io.IsAtEnd() && io.Peek() != '\n')
				io.NextChar();

			return true;
		}
	}

	internal sealed class ExactScanner : Scanner
	{
		public readonly string match;

		public ExactScanner(string match)
		{
			this.match = match;
		}

		public override bool Scan(TokenizerIO io)
		{
			return MatchStart(io, match);
		}
	}

	internal sealed class IntegerNumberScanner : Scanner
	{
		public override bool Scan(TokenizerIO io)
		{
			if (io.IsAtEnd() || !char.IsDigit(io.NextChar()))
				return false;

			while (!io.IsAtEnd() && char.IsDigit(io.NextChar()))
				continue;

			return true;
		}
	}

	internal sealed class RealNumberScanner : Scanner
	{
		public override bool Scan(TokenizerIO io)
		{
			if (io.IsAtEnd() || !char.IsDigit(io.NextChar()))
				return false;

			while (!io.IsAtEnd() && char.IsDigit(io.NextChar()))
				continue;

			if (io.IsAtEnd() || io.NextChar() != '.')
				return false;

			while (!io.IsAtEnd() && char.IsDigit(io.NextChar()))
				continue;

			return true;
		}
	}

	internal sealed class StringScanner : Scanner
	{
		public readonly char delimiter;

		public StringScanner(char delimiter)
		{
			this.delimiter = delimiter;
		}

		public override bool Scan(TokenizerIO io)
		{
			if (io.IsAtEnd() || io.NextChar() != delimiter)
				return false;

			while (!io.IsAtEnd())
			{
				var c = io.NextChar();
				if (c == delimiter)
					return true;
			}

			return false;
		}
	}

	internal sealed class IdentifierScanner : Scanner
	{
		public readonly string prefix;
		public readonly string extraChars;

		public IdentifierScanner(string prefix, string extraChars)
		{
			this.prefix = prefix;
			this.extraChars = extraChars;
		}

		public override bool Scan(TokenizerIO io)
		{
			if (!MatchStart(io, prefix) || io.IsAtEnd())
				return false;

			var firstCh = io.NextChar();
			if (!char.IsLetter(firstCh) && extraChars.IndexOf(firstCh) < 0)
				return false;

			while (io.IsAtEnd())
			{
				var c = io.NextChar();
				if (!char.IsLetter(c) && extraChars.IndexOf(c) < 0)
					break;
			}

			return true;
		}
	}
}