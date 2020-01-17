namespace Flow
{
	internal sealed class ParseRules
	{
		public delegate void PrefixFunction(Compiler compiler);
		public delegate void InfixFunction(Compiler compiler);

		private const int RuleCount = (int)TokenKind.COUNT;
		private readonly Precedence[] precedences = new Precedence[RuleCount];
		private readonly PrefixFunction[] prefixRules = new PrefixFunction[RuleCount];
		private readonly InfixFunction[] infixRules = new InfixFunction[RuleCount];

		public ParseRules()
		{
			void Set(TokenKind kind, PrefixFunction prefix, InfixFunction infix, Precedence precedence)
			{
				var index = (int)kind;
				precedences[index] = precedence;
				prefixRules[index] = prefix;
				infixRules[index] = infix;
			}

			Set(TokenKind.SemiColon, null, null, Precedence.None);
			Set(TokenKind.OpenParenthesis, Compiler.Grouping, null, Precedence.None);
			Set(TokenKind.OpenSquareBrackets, Compiler.ArrayExpression, null, Precedence.None);
			// Set(TokenKind.Minus, Compiler.Unary, Compiler.Binary, Precedence.Term);
			// Set(TokenKind.Plus, null, Compiler.Binary, Precedence.Term);
			// Set(TokenKind.Slash, null, Compiler.Binary, Precedence.Factor);
			// Set(TokenKind.Asterisk, null, Compiler.Binary, Precedence.Factor);
			// Set(TokenKind.Bang, Compiler.Unary, null, Precedence.None);
			// Set(TokenKind.EqualEqual, null, Compiler.Binary, Precedence.Equality);
			// Set(TokenKind.BangEqual, null, Compiler.Binary, Precedence.Equality);
			// Set(TokenKind.Greater, null, Compiler.Binary, Precedence.Comparison);
			// Set(TokenKind.GreaterEqual, null, Compiler.Binary, Precedence.Comparison);
			// Set(TokenKind.Less, null, Compiler.Binary, Precedence.Comparison);
			// Set(TokenKind.LessEqual, null, Compiler.Binary, Precedence.Comparison);
			Set(TokenKind.Pipe, null, Compiler.Pipe, Precedence.Pipe);
			Set(TokenKind.Identifier, Compiler.Command, null, Precedence.None);
			Set(TokenKind.Variable, Compiler.Variable, null, Precedence.None);
			Set(TokenKind.StringLiteral, Compiler.Literal, null, Precedence.None);
			Set(TokenKind.IntLiteral, Compiler.Literal, null, Precedence.None);
			Set(TokenKind.FloatLiteral, Compiler.Literal, null, Precedence.None);
			Set(TokenKind.Null, Compiler.Literal, null, Precedence.None);
			Set(TokenKind.False, Compiler.Literal, null, Precedence.None);
			Set(TokenKind.True, Compiler.Literal, null, Precedence.None);
		}

		public Precedence GetPrecedence(TokenKind kind)
		{
			return precedences[(int)kind];
		}

		public PrefixFunction GetPrefixRule(TokenKind kind)
		{
			return prefixRules[(int)kind];
		}

		public InfixFunction GetInfixRule(TokenKind kind)
		{
			return infixRules[(int)kind];
		}
	}
}