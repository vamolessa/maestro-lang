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

	internal struct ExpectedSemiColonAfterStatement : IFormattedMessage
	{
		public string Format() => "Expected ';' after statement";
	}

	internal struct ExpectedCloseParenthesisAfterExpression : IFormattedMessage
	{
		public string Format() => "Expected ')' after expression";
	}

	internal struct ExpectedCloseSquareBracketsAfterArrayExpression : IFormattedMessage
	{
		public string Format() => "Expected ']' after array expression";
	}

	internal struct TooManyArrayElementsError : IFormattedMessage
	{
		public string Format() => $"Too many array elements. Max is {byte.MaxValue}";
	}

	internal struct InvalidTokenAfterPipe : IFormattedMessage
	{
		public string Format() => "Expected variable or command after '|'";
	}

	internal struct TooManyCommandArgumentsError : IFormattedMessage
	{
		public string Format() => $"Too many command arguments. Max is {byte.MaxValue}";
	}

	internal struct CommandNotRegisteredError : IFormattedMessage
	{
		public string name;
		public string Format() => $"Command '{name}' not registered";
	}

	internal struct CanOnlyAssignVariablesAtTopLevelExpressions : IFormattedMessage
	{
		public string Format() => "Can only assign variables at top level expressions";
	}

	internal struct TooManyLocalVariables : IFormattedMessage
	{
		public string Format() => $"Too many variables. Max is {byte.MaxValue}";
	}

	internal struct LocalVariableUnassigned : IFormattedMessage
	{
		public string name;
		public string Format() => $"Use of unassigned '{name}' variable";
	}

	internal struct ExpectedLiteralError : IFormattedMessage
	{
		public TokenKind got;
		public string Format() => $"Expected literal. Got {got}";
	}

	internal struct LocalVariableNotUsed : IFormattedMessage
	{
		public string name;
		public string Format() => $"Variable '{name}' is never used";
	}
}