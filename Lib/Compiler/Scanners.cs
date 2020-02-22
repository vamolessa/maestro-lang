namespace Maestro
{
	internal abstract class Scanner
	{
		public TokenKind tokenKind;

		public static bool StartsWith(string str, int index, string match)
		{
			var count = str.Length;
			if (str.Length - index < match.Length)
				return false;

			for (var i = 0; i < match.Length; i++)
			{
				if (str[index + i] != match[i])
					return false;
			}

			return true;
		}

		public static int MatchFirstDigit(string str, int index)
		{
			if (index >= str.Length)
				return 0;

			var firstCh = str[index];
			if (char.IsDigit(firstCh))
				return 1;

			if (firstCh != '+' && firstCh != '-')
				return 0;

			index += 1;
			if (index >= str.Length)
				return 0;

			return char.IsDigit(str, index) ? 2 : 0;
		}

		public abstract int Scan(string input, int index);

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
		public override int Scan(string input, int index)
		{
			var startIndex = index;
			while (index < input.Length && char.IsWhiteSpace(input, index))
				index += 1;
			return index - startIndex;
		}
	}

	internal sealed class CharScanner : Scanner
	{
		public readonly char ch;

		public CharScanner(char ch)
		{
			this.ch = ch;
		}

		public override int Scan(string input, int index)
		{
			return input[index] == ch ? 1 : 0;
		}
	}

	internal sealed class ExactScanner : Scanner
	{
		public readonly string match;

		public ExactScanner(string match)
		{
			this.match = match;
		}

		public override int Scan(string input, int index)
		{
			return StartsWith(input, index, match) ? match.Length : 0;
		}
	}

	internal sealed class StringScanner : Scanner
	{
		public readonly char bracket;

		public StringScanner(char bracket)
		{
			this.bracket = bracket;
		}

		public override int Scan(string input, int index)
		{
			if (index >= input.Length || input[index] != bracket)
				return 0;

			for (var i = index + 1; i < input.Length; i++)
			{
				if (input[i] == bracket && input[i - 1] != '\\')
					return i - index + 1;
			}

			return 0;
		}
	}

	internal sealed class LineCommentScanner : Scanner
	{
		public readonly string prefix;

		public LineCommentScanner(string prefix)
		{
			this.prefix = prefix;
		}

		public override int Scan(string input, int index)
		{
			if (!StartsWith(input, index, prefix))
				return 0;

			for (var i = index + prefix.Length; i < input.Length; i++)
			{
				if (input[i] == '\n')
					return i - index;
			}

			return input.Length - index;
		}
	}

	internal sealed class IntegerNumberScanner : Scanner
	{
		public override int Scan(string input, int index)
		{
			var matchOffset = MatchFirstDigit(input, index);
			if (matchOffset == 0)
				return 0;

			for (var i = index + matchOffset; i < input.Length; i++)
			{
				if (!char.IsDigit(input, i))
					return i - index;
			}

			return input.Length - index;
		}
	}

	internal sealed class RealNumberScanner : Scanner
	{
		public override int Scan(string input, int index)
		{
			var matchOffset = MatchFirstDigit(input, index);
			if (matchOffset == 0)
				return 0;

			var i = index + matchOffset;
			for (; i < input.Length; i++)
			{
				if (!char.IsDigit(input, i))
					break;
			}

			if (i >= input.Length || input[i] != '.')
				return 0;
			i += 1;
			if (i >= input.Length || !char.IsDigit(input, i))
				return 0;

			for (; i < input.Length; i++)
			{
				if (!char.IsDigit(input, i))
					return i - index;
			}

			return input.Length - index;
		}
	}

	internal sealed class IdentifierScanner : Scanner
	{
		public readonly string prefix;
		public readonly string extraChars;

		public IdentifierScanner(string prefix, string additionalChars)
		{
			this.prefix = prefix;
			this.extraChars = additionalChars;
		}

		public override int Scan(string input, int index)
		{
			if (!StartsWith(input, index, prefix))
				return 0;

			index += prefix.Length;
			if (index >= input.Length)
				return 0;

			var firstCh = input[index];
			if (!char.IsLetter(firstCh) && extraChars.IndexOf(firstCh) < 0)
				return 0;

			for (var i = index + 1; i < input.Length; i++)
			{
				var ch = input[i];
				if (!char.IsLetterOrDigit(ch) && extraChars.IndexOf(ch) < 0)
					return i - index + prefix.Length;
			}

			return input.Length - index + prefix.Length;
		}
	}
}