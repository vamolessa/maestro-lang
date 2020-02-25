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

	namespace Assembly
	{
		internal struct TooManyDependencies : IFormattedMessage
		{
			public string Format() => $"Too many dependencies. Max is {byte.MaxValue}";
		}

		internal struct DependencyAssemblyNotFound : IFormattedMessage
		{
			public string dependencyUri;
			public string Format() => $"Could not find dependency assembly '{dependencyUri}'";
		}
	}

	namespace NativeCommands
	{
		internal struct WrongNumberOfNativeCommandArguments : IFormattedMessage
		{
			public string commandName;
			public int expected;
			public int got;
			public string Format() => $"Wrong number of arguments for native command '{commandName}'. Expected {expected}. Got {got}";
		}

		internal struct NativeCommandHasNoBinding : IFormattedMessage
		{
			public string name;
			public string Format() => $"Could not find a binding for native command '{name}'";
		}

		internal struct IncompatibleNativeCommand : IFormattedMessage
		{
			public string name;
			public int expectedParameterCount;
			public int gotParameterCount;

			public string Format() => $"Incompatible binding for native command '{name}'. Expected {expectedParameterCount} parameters. Got {gotParameterCount}";
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

		internal struct DuplicatedCommandParameterVariable : IFormattedMessage
		{
			public string commandName;
			public string parameterName;
			public string Format() => $"Command '{commandName}' already has a parameter variable named '{parameterName}'";
		}

		internal struct TooManyNativeCommandParameterVariables : IFormattedMessage
		{
			public string Format() => $"Too many command parameter variables. Max is {byte.MaxValue}";
		}

		internal struct ExpectedOpenCurlyBracesBeforeCommandBody : IFormattedMessage
		{
			public string Format() => "Expected '{' before command body";
		}

		internal struct CommandNameDuplicated : IFormattedMessage
		{
			public string name;
			public string Format() => $"There is alreay a command named '{name}'";
		}

		internal struct TooManyCommandsDefined : IFormattedMessage
		{
			public string Format() => $"Too many command defined. Max is {byte.MaxValue}";
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