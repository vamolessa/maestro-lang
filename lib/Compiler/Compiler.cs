[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("flow")]

namespace Flow
{
	internal sealed class Compiler
	{
		public readonly CompilerIO io = new CompilerIO();
		public Buffer<Source> compiledSources = new Buffer<Source>(1);

		private readonly ParseRules parseRules = new ParseRules();

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

		private void Syncronize()
		{
			if (!io.isInPanicMode)
				return;

			while (io.parser.currentToken.kind != TokenKind.End)
			{
				if (io.parser.currentToken.kind != TokenKind.SemiColon)
					io.parser.Next();

				io.isInPanicMode = false;
				break;
			}
		}

		private void ParseWithPrecedence(Precedence precedence)
		{
			var parser = io.parser;
			parser.Next();
			var slice = parser.previousToken.slice;
			if (parser.previousToken.kind == TokenKind.End)
				return;

			var prefixRule = parseRules.GetPrefixRule(parser.previousToken.kind);
			if (prefixRule == null)
			{
				io.AddHardError(parser.previousToken.slice, new ExpectedExpression());
				return;
			}
			prefixRule(this);

			while (
				parser.currentToken.kind != TokenKind.End &&
				precedence <= parseRules.GetPrecedence(parser.currentToken.kind)
			)
			{
				parser.Next();
				var infixRule = parseRules.GetInfixRule(parser.previousToken.kind);
				infixRule(this);
				slice = Slice.FromTo(slice, parser.previousToken.slice);
			}

			slice = Slice.FromTo(slice, parser.previousToken.slice);
		}

		private void Statement()
		{
			System.Console.WriteLine("BEGIN STATEMENT");
			ExpressionStatement();
		}

		private void ExpressionStatement()
		{
			Expression();
			io.parser.Consume(TokenKind.SemiColon, new ExpectedSemiColonAfterExpression());
		}

		private void Expression()
		{
			ParseWithPrecedence(Precedence.Pipe);
		}

		private void Value()
		{
			ParseWithPrecedence(Precedence.Primary);
		}

		internal static void Grouping(Compiler self)
		{
			self.Expression();
			self.io.parser.Consume(TokenKind.CloseParenthesis, new ExpectedCloseParenthesisAfterExpression());
		}

		internal static void ArrayExpression(Compiler self)
		{
			System.Console.WriteLine("[");
			while (!self.io.parser.Check(TokenKind.End))
			{
				self.Expression();
				if (!self.io.parser.Match(TokenKind.Comma))
					break;
			}

			self.io.parser.Consume(TokenKind.CloseSquareBrackets, new ExpectedCloseSquareBracketsAfterArrayExpression());

			System.Console.WriteLine("]");
		}

		internal static void Pipe(Compiler self)
		{
			System.Console.WriteLine("PIPE TO");
			self.ParseWithPrecedence(Precedence.Pipe);
		}

		internal static void Command(Compiler self)
		{
			var slice = self.io.parser.previousToken.slice;

			var argCount = 0;
			while (
				!self.io.parser.Check(TokenKind.End) &&
				!self.io.parser.Check(TokenKind.SemiColon) &&
				!self.io.parser.Check(TokenKind.Pipe) &&
				!self.io.parser.Check(TokenKind.CloseParenthesis) &&
				!self.io.parser.Check(TokenKind.CloseSquareBrackets)
			)
			{
				self.Value();
				argCount += 1;
			}

			System.Console.WriteLine("COMMAND {0} WITH {1} ARGS", self.io.parser.tokenizer.source.Substring(slice.index, slice.length), argCount);
		}

		internal static void Variable(Compiler self)
		{
			var slice = self.io.parser.previousToken.slice;
			System.Console.WriteLine("VARIABLE {0}", self.io.parser.tokenizer.source.Substring(self.io.parser.previousToken.slice.index, self.io.parser.previousToken.slice.length));
		}

		internal static void Literal(Compiler self)
		{
			switch (self.io.parser.previousToken.kind)
			{
			case TokenKind.False:
				System.Console.WriteLine("FALSE");
				break;
			case TokenKind.True:
				System.Console.WriteLine("TRUE");
				break;
			case TokenKind.IntLiteral:
				System.Console.WriteLine("INT {0}", self.io.parser.tokenizer.source.Substring(self.io.parser.previousToken.slice.index, self.io.parser.previousToken.slice.length));
				break;
			case TokenKind.FloatLiteral:
				System.Console.WriteLine("FLOAT {0}", self.io.parser.tokenizer.source.Substring(self.io.parser.previousToken.slice.index, self.io.parser.previousToken.slice.length));
				break;
			case TokenKind.StringLiteral:
				System.Console.WriteLine("STRING {0}", self.io.parser.tokenizer.source.Substring(self.io.parser.previousToken.slice.index, self.io.parser.previousToken.slice.length));
				break;
			default:
				self.io.AddHardError(self.io.parser.previousToken.slice, new ExpectedLiteralError { got = self.io.parser.previousToken.kind });
				break;
			}
		}
	}
}