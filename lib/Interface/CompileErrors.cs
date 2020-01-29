namespace Flow.CompileErrors
{
	namespace General
	{
		internal struct InvalidToken : IFormattedMessage
		{
			public string Format() => "Invalid char";
		}

		internal struct TooMuchCodeToJumpOver : IFormattedMessage
		{
			public string Format() => "Too much code to jump over";
		}

		internal struct ExpectedExpression : IFormattedMessage
		{
			public string Format() => "Expected expression";
		}

		internal struct ExpectedSemiColonAfterStatement : IFormattedMessage
		{
			public string Format() => "Expected ';' after statement expression";
		}
	}

	namespace Imports
	{
		internal struct ExpectedImportPathString : IFormattedMessage
		{
			public string Format() => "Expected import path string";
		}

		internal struct NoImportResolverProvided : IFormattedMessage
		{
			public string uri;
			public string Format() => $"No import resovler provided. Could not import '{uri}'";
		}

		internal struct CouldNotResolveImport : IFormattedMessage
		{
			public string importUri;
			public string fromUri;
			public string Format() => $"Could not resolve '{importUri}' from '{fromUri}'";
		}

		internal struct ExpectedSemiColonAfterImport : IFormattedMessage
		{
			public string Format() => "Expected ';' after import";
		}
	}

	namespace ExternalCommands
	{
		internal struct ExpectedExternalCommandIdentifier : IFormattedMessage
		{
			public string Format() => "Expected external command name";
		}

		internal struct ExpectedExternalCommandParameterCount : IFormattedMessage
		{
			public string Format() => "Expected external command parameter count number";
		}

		internal struct ExpectedExternalCommandReturnCount : IFormattedMessage
		{
			public string Format() => "Expected external command return count number";
		}

		internal struct TooManyExternalCommandParameters : IFormattedMessage
		{
			public string Format() => $"Too many external command parameters. Max is {byte.MaxValue}";
		}

		internal struct TooManyExternalCommandReturnValues : IFormattedMessage
		{
			public string Format() => $"Too many external command return values. Max is {byte.MaxValue}";
		}

		internal struct ExpectedSemiColonAfterExternCommand : IFormattedMessage
		{
			public string Format() => "Expected ';' after extern command";
		}
	}

	namespace Commands
	{
		internal struct WrongNumberOfCommandArguments : IFormattedMessage
		{
			public string commandName;
			public int expected;
			public int got;
			public string Format() => $"Wrong number of arguments for command '{commandName}'. Expected {expected}. Got {got}";
		}

		internal struct CommandNotRegistered : IFormattedMessage
		{
			public string name;
			public string Format() => $"Command '{name}' not registered";
		}

		internal struct CommandNameAlreadyRegistered : IFormattedMessage
		{
			public string name;
			public string Format() => $"Command name '{name}' already registered";
		}
	}

	namespace If
	{
		internal struct ExprectedOneValueAsIfCondition : IFormattedMessage
		{
			public int got;
			public string Format() => $"Expected one value as 'if' condition. Got {got}";
		}

		internal struct ExpectedOpenCurlyBracesAfterIfCondition : IFormattedMessage
		{
			public string Format() => "Expected '{' after 'if' condition";
		}

		internal struct ExpectedOpenCurlyBracesAfterElse : IFormattedMessage
		{
			public string Format() => "Expected '{' after else";
		}
	}

	namespace Iterate
	{
		internal struct ExpectedOneValueAsIterateCondition : IFormattedMessage
		{
			public int got;
			public string Format() => $"Expected one value as 'iterate' condition. Got {got}";
		}

		internal struct ExpectedOpenCurlyBracesAfterIterateCondition : IFormattedMessage
		{
			public string Format() => "Expected '{' after 'iterate' condition";
		}
	}

	namespace Block
	{
		internal struct ExpectedCloseCurlyBracketsAfterBlock : IFormattedMessage
		{
			public string Format() => "Expected '}' after block";
		}
	}

	namespace Group
	{
		internal struct ExpectedCloseParenthesisAfterExpression : IFormattedMessage
		{
			public string Format() => "Expected ')' after expression";
		}
	}

	namespace Pipe
	{
		internal struct InvalidTokenAfterPipe : IFormattedMessage
		{
			public string Format() => "Expected variable or command after '|'";
		}
	}

	namespace Comma
	{
		internal struct TooManyExpressionValues : IFormattedMessage
		{
			public string Format() => $"Too many expression values. Max is {byte.MaxValue}";
		}
	}

	namespace Variables
	{
		internal struct ExpectedVariableAsAssignmentTarget : IFormattedMessage
		{
			public string Format() => "Expected variable as assignment target";
		}

		internal struct CanOnlyAssignToVariablesAtTopLevelExpressions : IFormattedMessage
		{
			public string Format() => "Can only assign to variables at top level expressions";
		}

		internal struct MixedAssignmentType : IFormattedMessage
		{
			public string Format() => "Can not mix variable assignment and variable declaration";
		}

		internal struct WrongNumberOfVariablesOnAssignment : IFormattedMessage
		{
			public int expected;
			public int got;
			public string Format() => $"Wrong number of variables on assignment. Expected {expected}. Got {got}";
		}

		internal struct TooManyVariables : IFormattedMessage
		{
			public string Format() => $"Too many variables. Max is {byte.MaxValue}";
		}

		internal struct LocalVariableUnassigned : IFormattedMessage
		{
			public string name;
			public string Format() => $"Use of unassigned '{name}' variable";
		}

		internal struct NotReadLocalVariable : IFormattedMessage
		{
			public string name;
			public string Format() => $"Variable '{name}'s value is never read";
		}

		internal struct UnwrittenOutputVariable : IFormattedMessage
		{
			public string name;
			public string Format() => $"Output variable '{name}' is never written to";
		}
	}

	namespace Literals
	{
		internal struct ExpectedLiteral : IFormattedMessage
		{
			public TokenKind got;
			public string Format() => $"Expected literal. Got {got}";
		}
	}
}