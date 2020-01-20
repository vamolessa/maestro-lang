[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("flow")]

namespace Flow
{
	internal sealed class CompilerController
	{
		public readonly Compiler compiler = new Compiler();
		public Buffer<Source> compiledSources = new Buffer<Source>(1);

		private readonly ParseRules parseRules = new ParseRules();

		public Buffer<CompileError> CompileSource(ByteCodeChunk chunk, Source source)
		{
			compiledSources.count = 0;

			compiler.Reset(chunk);
			Compile(source);
			return compiler.errors;
		}

		private void Compile(Source source)
		{
			compiler.BeginSource(source.content, compiledSources.count);
			compiledSources.PushBack(source);

			//var finishedModuleImports = false;
			compiler.parser.Next();
			while (!compiler.parser.Match(TokenKind.End))
				Statement();

			for (var i = 0; i < compiler.localVariables.count; i++)
			{
				var local = compiler.localVariables.buffer[i];
				if (!local.used)
					compiler.AddSoftError(local.slice, new LocalVariableNotUsed { name = CompilerHelper.GetSlice(compiler, local.slice) });
			}

			compiler.EmitInstruction(Instruction.Pop);
			compiler.EmitByte((byte)compiler.localVariables.count);
			compiler.localVariables.count = 0;

			compiler.EndSource();
		}

		private void Syncronize()
		{
			if (!compiler.isInPanicMode)
				return;

			while (compiler.parser.currentToken.kind != TokenKind.End)
			{
				if (compiler.parser.currentToken.kind != TokenKind.SemiColon)
					compiler.parser.Next();

				compiler.isInPanicMode = false;
				break;
			}
		}

		private Slice ParseWithPrecedence(Precedence precedence)
		{
			var parser = compiler.parser;
			parser.Next();
			var slice = parser.previousToken.slice;
			if (parser.previousToken.kind == TokenKind.End)
				return slice;

			var prefixRule = parseRules.GetPrefixRule(parser.previousToken.kind);
			if (prefixRule == null)
			{
				compiler.AddHardError(parser.previousToken.slice, new ExpectedExpression());
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
				infixRule(this, slice);
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
			Expression(true);

			compiler.parser.Consume(TokenKind.SemiColon, new ExpectedSemiColonAfterStatement());
			compiler.EmitInstruction(Instruction.Pop);
			compiler.EmitByte(1);
		}

		private Slice Expression(bool canAssignToVariable)
		{
			var slice = ParseWithPrecedence(Precedence.Pipe);
			if (compiler.parser.Match(TokenKind.Pipe))
				Pipe(canAssignToVariable);

			return slice;
		}

		private Slice Value()
		{
			return ParseWithPrecedence(Precedence.Primary);
		}

		internal static void Grouping(CompilerController self)
		{
			self.Expression(false);
			self.compiler.parser.Consume(TokenKind.CloseParenthesis, new ExpectedCloseParenthesisAfterExpression());
		}

		internal static void ArrayExpression(CompilerController self)
		{
			var slice = self.compiler.parser.previousToken.slice;

			var elementCount = 0;
			while (!self.compiler.parser.Check(TokenKind.End))
			{
				self.Expression(false);
				elementCount += 1;

				if (!self.compiler.parser.Match(TokenKind.Comma))
					break;
			}

			self.compiler.parser.Consume(TokenKind.CloseSquareBrackets, new ExpectedCloseSquareBracketsAfterArrayExpression());
			slice = Slice.FromTo(slice, self.compiler.parser.previousToken.slice);

			if (elementCount > byte.MaxValue)
			{
				self.compiler.AddSoftError(slice, new TooManyArrayElementsError());
			}
			else
			{
				self.compiler.EmitInstruction(Instruction.CreateArray);
				self.compiler.EmitByte((byte)elementCount);
			}
		}

		private void Pipe(bool canAssignToVariable)
		{
			while (!compiler.parser.Check(TokenKind.End))
			{
				compiler.parser.Next();
				switch (compiler.parser.previousToken.kind)
				{
				case TokenKind.Variable:
					AssignLocal(canAssignToVariable);
					return;
				case TokenKind.Identifier:
					PipeCommand();
					break;
				default:
					compiler.AddHardError(compiler.parser.previousToken.slice, new InvalidTokenAfterPipe());
					return;
				}

				if (!compiler.parser.Match(TokenKind.Pipe))
					break;
			}
		}

		private bool IsLastPipeExpression()
		{
			return
				compiler.parser.Check(TokenKind.End) ||
				compiler.parser.Check(TokenKind.SemiColon) ||
				compiler.parser.Check(TokenKind.Pipe) ||
				compiler.parser.Check(TokenKind.CloseParenthesis) ||
				compiler.parser.Check(TokenKind.CloseSquareBrackets) ||
				compiler.parser.Check(TokenKind.Comma);
		}

		internal static void Command(CompilerController self)
		{
			self.compiler.EmitInstruction(Instruction.LoadNull);
			self.PipeCommand();
		}

		private void PipeCommand()
		{
			var commandSlice = compiler.parser.previousToken.slice;
			var slice = commandSlice;

			var argCount = 0;
			while (!IsLastPipeExpression())
			{
				var valueSlice = Value();
				slice = Slice.FromTo(slice, valueSlice);
				argCount += 1;
			}

			var commandIndex = -1;
			for (var i = 0; i < compiler.chunk.commands.count; i++)
			{
				var command = compiler.chunk.commands.buffer[i];
				if (CompilerHelper.AreEqual(
					compiler.parser.tokenizer.source,
					commandSlice,
					command.name
				))
				{
					commandIndex = i;
					break;
				}
			}

			if (commandIndex < 0)
			{
				compiler.AddSoftError(slice, new CommandNotRegisteredError { name = CompilerHelper.GetSlice(compiler, commandSlice) });
			}
			else
			{
				var command = compiler.chunk.commands.buffer[commandIndex];
				if (argCount != command.parameterCount)
					compiler.AddSoftError(slice, new WrongNumberOfCommandArgumentsError { commandName = command.name, expected = command.parameterCount, got = argCount });
				else
					compiler.EmitRunCommandInstance(commandIndex);
			}
		}

		private void AssignLocal(bool canAssign)
		{
			var slice = compiler.parser.previousToken.slice;

			if (!canAssign)
			{
				compiler.AddSoftError(slice, new CanOnlyAssignVariablesAtTopLevelExpressions());
				return;
			}

			if (compiler.ResolveToLocalVariableIndex(slice, out var localIndex))
			{
				compiler.EmitInstruction(Instruction.AssignLocal);
				compiler.EmitByte((byte)localIndex);
			}
			else
			{
				if (localIndex > byte.MaxValue)
				{
					compiler.AddSoftError(slice, new TooManyLocalVariables());
					return;
				}

				compiler.AddLocalVariable(slice, false);
				compiler.EmitInstruction(Instruction.LoadNull);
			}
		}

		internal static void LoadLocal(CompilerController self)
		{
			var slice = self.compiler.parser.previousToken.slice;

			if (self.compiler.ResolveToLocalVariableIndex(slice, out var localIndex))
			{
				self.compiler.localVariables.buffer[localIndex].used = true;

				self.compiler.EmitInstruction(Instruction.LoadLocal);
				self.compiler.EmitByte((byte)localIndex);
			}
			else
			{
				self.compiler.AddSoftError(slice, new LocalVariableUnassigned { name = CompilerHelper.GetSlice(self.compiler, slice) });
			}
		}

		internal static void Literal(CompilerController self)
		{
			switch (self.compiler.parser.previousToken.kind)
			{
			case TokenKind.Null:
				self.compiler.EmitInstruction(Instruction.LoadNull);
				break;
			case TokenKind.False:
				self.compiler.EmitInstruction(Instruction.LoadFalse);
				break;
			case TokenKind.True:
				self.compiler.EmitInstruction(Instruction.LoadTrue);
				break;
			case TokenKind.IntLiteral:
				self.compiler.EmitLoadLiteral(CompilerHelper.GetParsedInt(self.compiler));
				break;
			case TokenKind.FloatLiteral:
				self.compiler.EmitLoadLiteral(CompilerHelper.GetParsedFloat(self.compiler));
				break;
			case TokenKind.StringLiteral:
				self.compiler.EmitLoadLiteral(CompilerHelper.GetParsedString(self.compiler));
				break;
			default:
				self.compiler.AddHardError(self.compiler.parser.previousToken.slice, new ExpectedLiteralError { got = self.compiler.parser.previousToken.kind });
				break;
			}
		}
	}
}