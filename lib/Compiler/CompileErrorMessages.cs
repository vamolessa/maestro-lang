namespace Flow
{
	internal struct InvalidTokenError : IFormattedMessage
	{
		public string Format() => "Invalid char";
	}

	internal struct ExpectedVariableOnAssignment : IFormattedMessage
	{
		public string Format() => "Expected variable on assignment";
	}

	internal struct ExpectedEqualsOnAssignment : IFormattedMessage
	{
		public string Format() => "Expected '=' on assignment";
	}

	internal struct ExpectedSemiColonAtEndOfStatement : IFormattedMessage
	{
		public string Format() => "Expected ';' at end of statement";
	}

	internal struct ExpectedCommandNameError : IFormattedMessage
	{
		public TokenKind got;
		public string Format() => $"Expected command name. Got {got}";
	}

	internal struct ExpectedLiteralError : IFormattedMessage
	{
		public TokenKind got;
		public string Format() => $"Expected literal. Got {got}";
	}
}