namespace Rain
{
	internal sealed class TokenizerIO
	{
		public struct State
		{
			public int nextIndex;
			public int indentationLevel;
			public bool atBeginingOfLine;
			public int pendingIndentation;
		}

		public State state;
		public string source;

		public TokenizerIO()
		{
			source = string.Empty;
		}

		public bool IsAtEnd()
		{
			return state.indentationLevel == 0 && state.nextIndex >= source.Length;
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

			if (state.nextIndex >= source.Length && state.indentationLevel > 0)
				state.indentationLevel--;

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
				var indent = 0;
				while (source[state.nextIndex] == '\t')
				{
					indent += 1;
					state.nextIndex += 1;
				}
			}

			return source[state.nextIndex];
		}
	}
}
