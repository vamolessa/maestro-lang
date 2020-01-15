namespace Flow
{
	internal struct NeverError : IFormattedMessage
	{
		public string Format() => string.Empty;
	}

	internal struct InvalidTokenError : IFormattedMessage
	{
		public string Format() => "Invalid char";
	}

	internal struct ExpectedExpression : IFormattedMessage
	{
		public string Format() => "Expected expression";
	}

	internal struct ExpectedSemiColonAfterExpression : IFormattedMessage
	{
		public string Format() => "Expected ';' after expression";
	}

	internal struct ExpectedCommandNameError : IFormattedMessage
	{
		public string Format() => $"Expected command name";
	}

	internal struct ExpectedLiteralError : IFormattedMessage
	{
		public TokenKind got;
		public string Format() => $"Expected literal. Got {got}";
	}
}