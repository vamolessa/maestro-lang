namespace Flow
{
	internal static class TokenScanners
	{
		public static readonly Scanner[] scanners = new Scanner[] {
			new ExactScanner(";").ForToken(TokenKind.SemiColon),
			new ExactScanner("|").ForToken(TokenKind.Pipe),
			new ExactScanner("=").ForToken(TokenKind.Equals),

			new RealNumberScanner().ForToken(TokenKind.FloatLiteral),
			new IntegerNumberScanner().ForToken(TokenKind.IntLiteral),
			new StringScanner('"').ForToken(TokenKind.StringLiteral),
			new ExactScanner("true").ForToken(TokenKind.True),
			new ExactScanner("false").ForToken(TokenKind.False),
			new IdentifierScanner("", "_").ForToken(TokenKind.Identifier),
			new IdentifierScanner("$", "_").ForToken(TokenKind.Variable),

			new WhiteSpaceScanner().Ignore(),
			new LineCommentScanner("#").Ignore(),
		};
	}
}