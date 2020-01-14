[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("flow")]

namespace Flow
{
	internal sealed class Compiler
	{
		public readonly CompilerIO io = new CompilerIO();
		public Buffer<Source> compiledSources = new Buffer<Source>(1);

		public Buffer<CompileError> CompileSource(Source source)
		{
			compiledSources.count = 0;

			io.Reset();
			Compile(source);
			return io.errors;
		}

		private void Compile(Source source)
		{
			io.BeginSource(source.content, compiledSources.count);
			compiledSources.PushBack(source);

			//var finishedModuleImports = false;
			io.parser.Next();
			while (!io.parser.Match(TokenKind.End))
				Statement();

			io.EndSource();
		}

		private void Statement()
		{
			if (io.parser.Check(TokenKind.Variable))
				AssignmentStatement();
			else if (io.parser.Check(TokenKind.Identifier))
				ExpressionStatement();
		}

		private void AssignmentStatement()
		{
			io.parser.Consume(TokenKind.Variable, new ExpectedVariableOnAssignment());
			var variableSlice = io.parser.previousToken.slice;
			io.parser.Consume(TokenKind.Equals, new ExpectedEqualsOnAssignment());
		}

		private void ExpressionStatement()
		{
			Expression();
			io.parser.Consume(TokenKind.SemiColon, new ExpectedSemiColonAtEndOfStatement());
		}

		private void Expression()
		{
			Command();
		}

		private void Command()
		{
			io.parser.Consume(TokenKind.Identifier, new ExpectedCommandNameError());
		}

		private void Value()
		{
			Literal();
		}

		private void Literal()
		{
			io.parser.Next();

			switch (io.parser.previousToken.kind)
			{
			case TokenKind.False:
				System.Console.WriteLine("FALSE");
				break;
			case TokenKind.True:
				System.Console.WriteLine("TRUE");
				break;
			case TokenKind.IntLiteral:
				System.Console.WriteLine("INT {0}", io.parser.tokenizer.source.Substring(io.parser.previousToken.slice.index, io.parser.previousToken.slice.length));
				break;
			case TokenKind.FloatLiteral:
				System.Console.WriteLine("FLOAT {0}", io.parser.tokenizer.source.Substring(io.parser.previousToken.slice.index, io.parser.previousToken.slice.length));
				break;
			case TokenKind.StringLiteral:
				System.Console.WriteLine("STRING {0}", io.parser.tokenizer.source.Substring(io.parser.previousToken.slice.index, io.parser.previousToken.slice.length));
				break;
			default:
				io.AddHardError(io.parser.previousToken.slice, new ExpectedLiteralError { got = io.parser.previousToken.kind });
				break;
			}
		}
	}
}