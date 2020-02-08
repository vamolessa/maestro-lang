namespace Maestro.CompileErrors
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

	namespace ExternCommands
	{
		internal struct ExpectedCommandKeyword : IFormattedMessage
		{
			public string Format() => "Expected 'command' keyword";
		}

		internal struct ExpectedExternCommandIdentifier : IFormattedMessage
		{
			public string Format() => "Expected extern command name";
		}

		internal struct ExpectedExternCommandParameterCount : IFormattedMessage
		{
			public string Format() => "Expected extern command parameter count number";
		}

		internal struct TooManyExternCommandParameters : IFormattedMessage
		{
			public string Format() => $"Too many extern command parameters. Max is {byte.MaxValue}";
		}

		internal struct ExpectedSemiColonAfterExternCommand : IFormattedMessage
		{
			public string Format() => "Expected ';' after extern command declaration";
		}

		internal struct WrongNumberOfExternCommandArguments : IFormattedMessage
		{
			public string commandName;
			public int expected;
			public int got;
			public string Format() => $"Wrong number of arguments for extern command '{commandName}'. Expected {expected}. Got {got}";
		}

		internal struct ExternCommandHasNoBinding : IFormattedMessage
		{
			public string name;
			public string Format() => $"Could not find a binding for extern command '{name}'";
		}

		internal struct IncompatibleExternCommand : IFormattedMessage
		{
			public string name;
			public int expectedParameterCount;
			public int gotParameterCount;

			public string Format() => $"Incompatible binding for extern command '{name}'. Expected {expectedParameterCount} parameters. Got {gotParameterCount}";
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

		internal struct TooManyExternCommandParameterVariables : IFormattedMessage
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

		internal struct CommandNameDuplicated : IFormattedMessage
		{
			public string name;
			public string Format() => $"There is alreay a command named '{name}'";
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

	namespace ForEach
	{
		internal struct ExpectedForEachVariable : IFormattedMessage
		{
			public string Format() => "Expected 'foreach' iteration variable";
		}

		internal struct ExpectedInAfterForEachVariable : IFormattedMessage
		{
			public string Format() => "Expected 'in' after 'foreach' iteration variable";
		}

		internal struct ExpectedOpenCurlyBracesAfterForEachExpression : IFormattedMessage
		{
			public string Format() => "Expected '{' after 'foreach' expression";
		}
	}

	namespace Return
	{
		internal struct ExpectedSemiColonAfterReturn : IFormattedMessage
		{
			public string Format() => "Expected ';' after return";
		}

		internal struct CanNotReturnFromOutsideCommand : IFormattedMessage
		{
			public string Format() => "Can not return from outside a command";
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
		internal struct CanNotAccessVariableOutsideOfCommandScope : IFormattedMessage
		{
			public string name;
			public string Format() => $"Can not access variable '{name}' outside of command scope";
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

		internal struct VariableUnassigned : IFormattedMessage
		{
			public string name;
			public string Format() => $"Use of unassigned '{name}' variable";
		}

		internal struct NotReadVariable : IFormattedMessage
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