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

		internal struct ExpectedSemiColonAfterExpression : IFormattedMessage
		{
			public string Format() => "Expected ';' after expression";
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
		internal struct ExpectedCommandKeyword : IFormattedMessage
		{
			public string Format() => "Expected 'command' keyword";
		}

		internal struct ExpectedExternalCommandIdentifier : IFormattedMessage
		{
			public string Format() => "Expected external command name";
		}

		internal struct ExpectedExternalCommandParameterCount : IFormattedMessage
		{
			public string Format() => "Expected external command parameter count number";
		}

		internal struct TooManyExternalCommandParameters : IFormattedMessage
		{
			public string Format() => $"Too many external command parameters. Max is {byte.MaxValue}";
		}

		internal struct ExpectedSemiColonAfterExternCommand : IFormattedMessage
		{
			public string Format() => "Expected ';' after extern command declaration";
		}

		internal struct WrongNumberOfExternalCommandArguments : IFormattedMessage
		{
			public string commandName;
			public int expected;
			public int got;
			public string Format() => $"Wrong number of arguments for external command '{commandName}'. Expected {expected}. Got {got}";
		}
	}

	namespace Commands
	{
		internal struct ExpectedCommandIdentifier : IFormattedMessage
		{
			public string Format() => "Expected command name";
		}

		internal struct ExpectedCommandParameterVariable : IFormattedMessage
		{
			public string Format() => "Expected command parameter variable";
		}

		internal struct TooManyExternalCommandParameterVariables : IFormattedMessage
		{
			public string Format() => $"Too many command parameter variables. Max is {byte.MaxValue}";
		}

		internal struct ExpectedOpenCurlyBracesBeforeCommandBody : IFormattedMessage
		{
			public string Format() => "Expected '{' before command body";
		}

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

		internal struct ExpectedSemiColonAfterReturn : IFormattedMessage
		{
			public string Format() => "Expected ';' after return";
		}
	}

	namespace If
	{
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

	namespace Variables
	{
		internal struct CanNotAccessVariableOutsideOfScope : IFormattedMessage
		{
			public string name;
			public string Format() => $"Can not access variable '{name}' outside of current scope";
		}

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

		internal struct TooManyVariablesOnAssignment : IFormattedMessage
		{
			public string Format() => $"Too many variables on assignment. Max is {byte.MaxValue}";
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