namespace Flow
{
	internal static class TokenScanners
	{
		public static readonly Scanner[] scanners = new Scanner[] {
			new ExactScanner("\n").ForToken(TokenKind.NewLine),
			new ExactScanner("\t").ForToken(TokenKind.Indent),
			new ExactScanner("\b").ForToken(TokenKind.Dedent),

			new RealNumberScanner().ForToken(TokenKind.FloatLiteral),
			new IntegerNumberScanner().ForToken(TokenKind.IntLiteral),
			new StringScanner('"').ForToken(TokenKind.StringLiteral),
			new ExactScanner("true").ForToken(TokenKind.True),
			new ExactScanner("false").ForToken(TokenKind.False),
			new IdentifierScanner("", "_").ForToken(TokenKind.Identifier),
			new IdentifierScanner("$", "_").ForToken(TokenKind.Variable),

			new ExactScanner("do").ForToken(TokenKind.Do),

			new WhiteSpaceScanner("\n\t").Ignore(),
			new LineCommentScanner("#").Ignore(),
		};
	}
}