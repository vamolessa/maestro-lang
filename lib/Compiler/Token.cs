namespace Flow
{
	internal enum TokenKind
	{
		UNDEFINED,
		SemiColon, Pipe, Comma,
		OpenParenthesis, CloseParenthesis,
		OpenSquareBrackets, CloseSquareBrackets,
		OpenCurlyBrackets, CloseCurlyBrackets,

		IntLiteral, FloatLiteral, StringLiteral, Null, True, False,
		Identifier, Variable,

		If, Else,

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
		Pipe,
		// Equality, // == !=
		// Comparison, // < > <= >=
		Term,// + -
		Factor, // * /
		Unary, // ! -
		Primary,
	}
}
