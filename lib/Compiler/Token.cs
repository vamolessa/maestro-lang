namespace Rain
{
	internal enum CharKind
	{
		Char,
		Indent,
		Dedent
	}

	internal readonly struct Char
	{
		public readonly CharKind kind;
		public readonly char value;

		public Char(CharKind kind, char value)
		{
			this.kind = kind;
			this.value = value;
		}

		public Char(CharKind kind)
		{
			this.kind = kind;
			this.value = default;
		}
	}

	internal enum TokenKind
	{
		NewLine, Indent, Dedent,
		IntLiteral, FloatLiteral, StringLiteral, True, False,
		Identifier, Variable,
		Do,

		COUNT,
		End,
		Error,
	}

	internal readonly struct Token
	{
		public readonly TokenKind kind;
		public readonly Slice slice;

		public Token(TokenKind kind, Slice slice)
		{
			this.kind = kind;
			this.slice = slice;
		}

		public bool IsValid()
		{
			return kind >= 0;
		}
	}
}