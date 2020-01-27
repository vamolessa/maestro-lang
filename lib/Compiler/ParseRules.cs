namespace Flow
{
	internal sealed class ParseRules
	{
		public delegate byte PrefixFunction(CompilerController controller);
		public delegate byte InfixFunction(CompilerController controller, ExpressionResult result);

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
			Set(TokenKind.OpenParenthesis, CompilerController.Grouping, null, Precedence.None);
			Set(TokenKind.OpenCurlyBrackets, null, null, Precedence.None);
			Set(TokenKind.CloseParenthesis, null, null, Precedence.None);
			Set(TokenKind.CloseCurlyBrackets, null, null, Precedence.None);
			Set(TokenKind.Pipe, null, null, Precedence.None);
			Set(TokenKind.Comma, null, CompilerController.Comma, Precedence.Comma);
			Set(TokenKind.If, null, null, Precedence.None);
			Set(TokenKind.Else, null, null, Precedence.None);
			Set(TokenKind.Identifier, CompilerController.Command, null, Precedence.None);
			Set(TokenKind.Variable, CompilerController.LoadLocal, null, Precedence.None);
			Set(TokenKind.InputVariable, CompilerController.LoadLocal, null, Precedence.None);
			Set(TokenKind.StringLiteral, CompilerController.Literal, null, Precedence.None);
			Set(TokenKind.IntLiteral, CompilerController.Literal, null, Precedence.None);
			Set(TokenKind.FloatLiteral, CompilerController.Literal, null, Precedence.None);
			Set(TokenKind.False, CompilerController.Literal, null, Precedence.None);
			Set(TokenKind.True, CompilerController.Literal, null, Precedence.None);
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