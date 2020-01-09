namespace Rain
{
	internal sealed class TokenizerIO
	{
		public struct State
		{
			public int nextIndex;
			public bool atBeginingOfLine;
			public int indentation;
			public int pendingIndentation;
		}

		public State state;
		public string source;

		public TokenizerIO()
		{
			source = string.Empty;
			state.atBeginingOfLine = true;
		}

		public bool IsAtEnd()
		{
			return state.indentation == 0 && state.nextIndex >= source.Length;
		}

		public char NextChar()
		{
			var c = Peek();
			if (state.pendingIndentation > 0)
				state.pendingIndentation--;
			else if (state.pendingIndentation < 0)
				state.pendingIndentation++;
			else
				state.nextIndex++;

			if (state.nextIndex >= source.Length && state.indentation > 0)
				state.indentation--;

			return c;
		}

		public char Peek()
		{
			if (state.pendingIndentation > 0)
				return '\t';
			else if (state.pendingIndentation < 0 || state.nextIndex >= source.Length)
				return '\b';

			if (state.atBeginingOfLine)
			{
			BeginingOfLine:
				state.atBeginingOfLine = false;

				var indent = 0;
				while (source[state.nextIndex] == '\t')
				{
					indent += 1;
					state.nextIndex += 1;
				}

				while (true)
				{
					var c = source[state.nextIndex];
					if (!char.IsWhiteSpace(c))
						break;

					state.nextIndex += 1;
					if (c == '\n')
						goto BeginingOfLine;
				}

				state.pendingIndentation = indent - state.indentation;
				if (state.pendingIndentation == 0 && indent > 0)
					state.nextIndex -= 1;

				state.indentation = indent;

				if (state.pendingIndentation > 0)
					return '\t';
				else if (state.pendingIndentation < 0)
					return '\b';
			}

			{
				var c = source[state.nextIndex];
				state.atBeginingOfLine = c == '\n';

				return c;
			}
		}
	}
}
