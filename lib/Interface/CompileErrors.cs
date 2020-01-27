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

	internal struct TooMuchCodeToJumpOverError : IFormattedMessage
	{
		public string Format() => "Too much code to jump over";
	}

	internal struct ExpectedExpressionError : IFormattedMessage
	{
		public string Format() => "Expected expression";
	}

	internal struct ExpectedSemiColonAfterStatementError : IFormattedMessage
	{
		public string Format() => "Expected ';' after statement expression";
	}

	internal struct ExpectedCloseCurlyBracketsAfterBlockError : IFormattedMessage
	{
		public string Format() => "Expected '}' after block";
	}

	internal struct ExpectedCloseParenthesisAfterExpressionError : IFormattedMessage
	{
		public string Format() => "Expected ')' after expression";
	}

	internal struct TooManyExpressionValuesError : IFormattedMessage
	{
		public string Format() => $"Too many expression values. Max is {byte.MaxValue}";
	}

	internal struct InvalidTokenAfterPipeError : IFormattedMessage
	{
		public string Format() => "Expected variable or command after '|'";
	}

	internal struct ExprectedOneValueAsIfConditionError : IFormattedMessage
	{
		public int got;
		public string Format() => $"Expected one value as if condition. Got {got}";
	}

	internal struct ExpectedOpenCurlyBracesAfterIfConditionError : IFormattedMessage
	{
		public string Format() => "Expected '{' after if condition";
	}

	internal struct ExpectedOpenCurlyBracesAfterElseError : IFormattedMessage
	{
		public string Format() => "Expected '{' after else";
	}

	internal struct ExpectedTwoValuesAsIterateConditionError : IFormattedMessage
	{
		public int got;
		public string Format() => $"Expected two values as iterate condition. Got {got}";
	}

	internal struct ExpectedOpenCurlyBracesAfterIterateConditionError : IFormattedMessage
	{
		public string Format() => "Expected '{' after iterate condition";
	}

	internal struct WrongNumberOfCommandArgumentsError : IFormattedMessage
	{
		public string commandName;
		public int expected;
		public int got;
		public string Format() => $"Wrong number of arguments for command '{commandName}'. Expected {expected}. Got {got}";
	}

	internal struct CommandNotRegisteredError : IFormattedMessage
	{
		public string name;
		public string Format() => $"Command '{name}' not registered";
	}

	internal struct CanOnlyAssignVariablesAtTopLevelExpressionsError : IFormattedMessage
	{
		public string Format() => "Can only assign variables at top level expressions";
	}

	internal struct ExpectedOneValueToAssignToVariableError : IFormattedMessage
	{
		public string variableName;
		public int got;
		public string Format() => $"Expected one value to assign to variable '{variableName}'. Got {got}";
	}

	internal struct TooManyVariablesError : IFormattedMessage
	{
		public string Format() => $"Too many variables. Max is {byte.MaxValue}";
	}

	internal struct LocalVariableUnassignedError : IFormattedMessage
	{
		public string name;
		public string Format() => $"Use of unassigned '{name}' variable";
	}

	internal struct ExpectedLiteralError : IFormattedMessage
	{
		public TokenKind got;
		public string Format() => $"Expected literal. Got {got}";
	}

	internal struct LocalVariableNotUsedError : IFormattedMessage
	{
		public string name;
		public string Format() => $"Variable '{name}' is never used";
	}
}