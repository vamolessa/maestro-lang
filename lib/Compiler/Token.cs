namespace Flow
{
	internal enum TokenKind
	{
		UNDEFINED,
		SemiColon, Pipe, Comma,
		OpenParenthesis, CloseParenthesis,
		OpenCurlyBrackets, CloseCurlyBrackets,

		IntLiteral, FloatLiteral, StringLiteral, True, False,
		Identifier, Variable, InputVariable,

		Import, If, Else, Iterate, Command, External, Arrow,

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

	internal enum Precedence
	{
		None,
		Expression,
		Comma,
		Primary,
	}
}
