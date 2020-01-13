namespace Flow
{
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
