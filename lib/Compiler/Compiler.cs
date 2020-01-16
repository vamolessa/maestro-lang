[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("flow")]

namespace Flow
{
	internal sealed class Compiler
	{
		public readonly CompilerIO io = new CompilerIO();
		public Buffer<Source> compiledSources = new Buffer<Source>(1);

		private readonly ParseRules parseRules = new ParseRules();

		public Buffer<CompileError> CompileSource(ByteCodeChunk chunk, Source source)
		{
			compiledSources.count = 0;

			io.Reset(chunk);
			Compile(source);
			return io.errors;
		}

		private void Compile(Source source)
		{
			io.BeginSource(source.content, compiledSources.count);
			compiledSources.PushBack(source);

			io.EmitInstruction(Instruction.ClearVariables);

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

		private Slice ParseWithPrecedence(Precedence precedence)
		{
			var parser = io.parser;
			parser.Next();
			var slice = parser.previousToken.slice;
			if (parser.previousToken.kind == TokenKind.End)
				return slice;

			var prefixRule = parseRules.GetPrefixRule(parser.previousToken.kind);
			if (prefixRule == null)
			{
				io.AddHardError(parser.previousToken.slice, new ExpectedExpression());
				return slice;
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
			return slice;
		}

		private void Statement()
		{
			ExpressionStatement();
		}

		private void ExpressionStatement()
		{
			Expression();
			io.parser.Consume(TokenKind.SemiColon, new ExpectedSemiColonAfterExpression());
			io.EmitInstruction(Instruction.ClearStack);
		}

		private Slice Expression()
		{
			return ParseWithPrecedence(Precedence.Pipe);
		}

		private Slice Value()
		{
			return ParseWithPrecedence(Precedence.Primary);
		}

		internal static void Grouping(Compiler self)
		{
			self.Expression();
			self.io.parser.Consume(TokenKind.CloseParenthesis, new ExpectedCloseParenthesisAfterExpression());
		}

		internal static void ArrayExpression(Compiler self)
		{
			while (!self.io.parser.Check(TokenKind.End))
			{
				self.Expression();
				if (!self.io.parser.Match(TokenKind.Comma))
					break;
			}

			self.io.parser.Consume(TokenKind.CloseSquareBrackets, new ExpectedCloseSquareBracketsAfterArrayExpression());
		}

		internal static void Pipe(Compiler self)
		{
			self.ParseWithPrecedence(Precedence.Pipe);
		}

		private bool IsLastPipeExpression()
		{
			return
				io.parser.Check(TokenKind.End) ||
				io.parser.Check(TokenKind.SemiColon) ||
				io.parser.Check(TokenKind.Pipe) ||
				io.parser.Check(TokenKind.CloseParenthesis) ||
				io.parser.Check(TokenKind.CloseSquareBrackets) ||
				io.parser.Check(TokenKind.Comma);
		}

		internal static void Command(Compiler self)
		{
			var commandSlice = self.io.parser.previousToken.slice;
			var slice = commandSlice;

			var argCount = 0;
			while (!self.IsLastPipeExpression())
			{
				var valueSlice = self.Value();
				slice = Slice.FromTo(slice, valueSlice);
				argCount += 1;
			}

			var commandIndex = -1;
			for (var i = 0; i < self.io.chunk.commands.count; i++)
			{
				var command = self.io.chunk.commands.buffer[i];
				if (CompilerHelper.AreEqual(
					self.io.parser.tokenizer.source,
					commandSlice,
					command.name
				))
				{
					commandIndex = i;
					break;
				}
			}

			if (argCount > byte.MaxValue)
				self.io.AddSoftError(slice, new TooManyCommandArguments());
			else if (commandIndex < 0)
				self.io.AddSoftError(slice, new CommandNotRegisteredError { name = CompilerHelper.GetSlice(self.io, commandSlice) });
			else
				self.io.EmitRunCommandInstance(commandIndex, (byte)argCount);
		}

		internal static void Variable(Compiler self)
		{
			var slice = self.io.parser.previousToken.slice;
			var name = CompilerHelper.GetSlice(self.io, slice);
			self.io.EmitVariableInstruction(Instruction.LoadVariable, name);
		}

		internal static void Literal(Compiler self)
		{
			switch (self.io.parser.previousToken.kind)
			{
			case TokenKind.False:
				self.io.EmitInstruction(Instruction.LoadFalse);
				break;
			case TokenKind.True:
				self.io.EmitInstruction(Instruction.LoadTrue);
				break;
			case TokenKind.IntLiteral:
				self.io.EmitLoadLiteral(CompilerHelper.GetParsedInt(self.io));
				break;
			case TokenKind.FloatLiteral:
				self.io.EmitLoadLiteral(CompilerHelper.GetParsedFloat(self.io));
				break;
			case TokenKind.StringLiteral:
				self.io.EmitLoadLiteral(CompilerHelper.GetParsedString(self.io));
				break;
			default:
				self.io.AddHardError(self.io.parser.previousToken.slice, new ExpectedLiteralError { got = self.io.parser.previousToken.kind });
				break;
			}
		}
	}
}