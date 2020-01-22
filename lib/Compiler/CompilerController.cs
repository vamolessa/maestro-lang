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

		private bool TryParseWithPrecedence(Precedence precedence, out Slice slice)
		{
			var parser = compiler.parser;
			slice = parser.currentToken.slice;
			if (parser.currentToken.kind == TokenKind.End)
			{
				parser.Next();
				return true;
			}

			var prefixRule = parseRules.GetPrefixRule(parser.currentToken.kind);
			if (prefixRule == null)
				return false;
			parser.Next();
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
			return true;
		}

		private void Statement()
		{
			if (compiler.parser.Match(TokenKind.If))
				IfStatement();
			else
				ExpressionStatement();
		}

		private void ExpressionStatement()
		{
			var slice = Expression(true);

			compiler.parser.Next();
			if (compiler.parser.previousToken.kind != TokenKind.SemiColon)
				compiler.AddHardError(slice, new ExpectedSemiColonAfterStatementError());
			compiler.EmitInstruction(Instruction.Pop);
		}

		private Slice Expression(bool canAssignToVariable)
		{
			if (!TryParseWithPrecedence(Precedence.Pipe, out var slice))
				compiler.AddHardError(compiler.parser.previousToken.slice, new ExpectedExpressionError());

			if (compiler.parser.Match(TokenKind.Pipe))
			{
				var pipeSlice = Pipe(canAssignToVariable);
				slice = Slice.FromTo(slice, pipeSlice);
			}

			return slice;
		}

		private bool TryValue(out Slice slice)
		{
			return TryParseWithPrecedence(Precedence.Primary, out slice);
		}

		private void IfStatement()
		{
			var expressionSlice = Expression(false);

			compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new ExpectedOpenCurlyBracesAfterIfConditionError());

			var elseJump = compiler.BeginEmitForwardJump(Instruction.PopAndJumpForwardIfFalse);

			Block();

			var thenJump = compiler.BeginEmitForwardJump(Instruction.JumpForward);
			compiler.EndEmitForwardJump(elseJump);

			if (compiler.parser.Match(TokenKind.Else))
			{
				if (compiler.parser.Match(TokenKind.If))
				{
					IfStatement();
				}
				else
				{
					compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new ExpectedOpenCurlyBracesAfterElseError());
					Block();
				}
			}

			compiler.EndEmitForwardJump(thenJump);
		}

		private void Block()
		{
			var scope = compiler.BeginScope();
			while (
				!compiler.parser.Check(TokenKind.End) &&
				!compiler.parser.Check(TokenKind.CloseCurlyBrackets)
			)
			{
				Statement();
			}

			compiler.parser.Consume(TokenKind.CloseCurlyBrackets, new ExpectedCloseCurlyBracketsAfterBlockError());
			compiler.EndScope(scope);
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

			self.compiler.parser.Consume(TokenKind.CloseSquareBrackets, new ExpectedCloseSquareBracketsAfterArrayExpressionError());
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

		private Slice Pipe(bool canAssignToVariable)
		{
			var slice = compiler.parser.previousToken.slice;

			while (!compiler.parser.Check(TokenKind.End))
			{
				compiler.parser.Next();
				switch (compiler.parser.previousToken.kind)
				{
				case TokenKind.Variable:
					slice = Slice.FromTo(slice, compiler.parser.previousToken.slice);
					AssignLocal(canAssignToVariable);
					return slice;
				case TokenKind.Identifier:
					{
						var pipeSlice = PipeCommand();
						slice = Slice.FromTo(slice, pipeSlice);
						break;
					}
				default:
					compiler.AddHardError(compiler.parser.previousToken.slice, new InvalidTokenAfterPipeError());
					return slice;
				}

				if (!compiler.parser.Match(TokenKind.Pipe))
					break;
			}

			return slice;
		}

		internal static void Command(CompilerController self)
		{
			self.compiler.EmitInstruction(Instruction.LoadNull);
			self.PipeCommand();
		}

		private Slice PipeCommand()
		{
			var commandSlice = compiler.parser.previousToken.slice;
			var slice = commandSlice;

			var argCount = 0;
			while (TryValue(out var valueSlice))
			{
				slice = Slice.FromTo(slice, valueSlice);
				argCount += 1;
			}

			var commandIndex = -1;
			for (var i = 0; i < compiler.chunk.commandDefinitions.count; i++)
			{
				var command = compiler.chunk.commandDefinitions.buffer[i];
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
				var command = compiler.chunk.commandDefinitions.buffer[commandIndex];
				if (argCount != command.parameterCount)
					compiler.AddSoftError(slice, new WrongNumberOfCommandArgumentsError { commandName = command.name, expected = command.parameterCount, got = argCount });
				else
					compiler.EmitCallNativeCommand(commandIndex);
			}

			return slice;
		}

		private void AssignLocal(bool canAssign)
		{
			var slice = compiler.parser.previousToken.slice;

			if (!canAssign)
			{
				compiler.AddSoftError(slice, new CanOnlyAssignVariablesAtTopLevelExpressionsError());
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
					compiler.AddSoftError(slice, new TooManyLocalVariablesError());
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
				self.compiler.AddSoftError(slice, new LocalVariableUnassignedError { name = CompilerHelper.GetSlice(self.compiler, slice) });
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
				self.compiler.EmitLoadLiteral(new Value(CompilerHelper.GetParsedInt(self.compiler)));
				break;
			case TokenKind.FloatLiteral:
				self.compiler.EmitLoadLiteral(new Value(CompilerHelper.GetParsedFloat(self.compiler)));
				break;
			case TokenKind.StringLiteral:
				self.compiler.EmitLoadLiteral(new Value(CompilerHelper.GetParsedString(self.compiler)));
				break;
			default:
				self.compiler.AddHardError(self.compiler.parser.previousToken.slice, new ExpectedLiteralError { got = self.compiler.parser.previousToken.kind });
				break;
			}
		}
	}
}