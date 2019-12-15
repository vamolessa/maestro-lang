namespace Rain
{
	internal static class TokenScanners
	{
		public static readonly Scanner[] scanners = new Scanner[] {
			new ExactScanner("\n").ForToken(TokenKind.NewLine),
			new ExactScanner("\t").ForToken(TokenKind.Tab),

			new RealNumberScanner().ForToken(TokenKind.FloatLiteral),
			new IntegerNumberScanner().ForToken(TokenKind.IntLiteral),
			new EnclosedScanner("\"", "\"").ForToken(TokenKind.StringLiteral),
			new ExactScanner("true").ForToken(TokenKind.True),
			new ExactScanner("false").ForToken(TokenKind.False),
			new IdentifierScanner("_").ForToken(TokenKind.Identifier),
			new PrefixedIdentifierScanner("$", "_").ForToken(TokenKind.Variable),

			new ExactScanner("do").ForToken(TokenKind.Do),

			new WhiteSpaceScanner().Ignore(),
			new EnclosedScanner("#", "\n").Ignore(),
		};
	}
}