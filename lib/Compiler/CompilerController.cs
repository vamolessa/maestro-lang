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

		private bool TryParseWithPrecedence(Precedence precedence, out ExpressionResult result)
		{
			var parser = compiler.parser;
			var slice = parser.currentToken.slice;
			if (parser.currentToken.kind == TokenKind.End)
			{
				parser.Next();
				result = new ExpressionResult(slice, 0);
				return true;
			}

			var prefixRule = parseRules.GetPrefixRule(parser.currentToken.kind);
			if (prefixRule == null)
			{
				result = new ExpressionResult(slice, 0);
				return false;
			}

			parser.Next();
			var valueCount = prefixRule(this);

			while (
				parser.currentToken.kind != TokenKind.End &&
				precedence <= parseRules.GetPrecedence(parser.currentToken.kind)
			)
			{
				parser.Next();
				var infixRule = parseRules.GetInfixRule(parser.previousToken.kind);
				valueCount = infixRule(this, new ExpressionResult(slice, valueCount));
				slice = Slice.FromTo(slice, parser.previousToken.slice);
			}

			slice = Slice.FromTo(slice, parser.previousToken.slice);
			result = new ExpressionResult(slice, valueCount);
			return true;
		}

		private void Statement()
		{
			if (compiler.parser.Match(TokenKind.If))
				IfStatement();
			else if (compiler.parser.Match(TokenKind.Iterate))
				IterateStatement();
			else
				ExpressionStatement();
		}

		private void ExpressionStatement()
		{
			var expression = Expression(true);

			compiler.parser.Next();
			if (compiler.parser.previousToken.kind != TokenKind.SemiColon)
				compiler.AddHardError(expression.slice, new ExpectedSemiColonAfterStatementError());

			compiler.EmitPop(expression.valueCount);
		}

		private ExpressionResult Expression(bool canAssignToVariable)
		{
			if (!TryParseWithPrecedence(Precedence.Pipe, out var result))
				compiler.AddHardError(compiler.parser.previousToken.slice, new ExpectedExpressionError());

			if (compiler.parser.Match(TokenKind.Pipe))
				result = Pipe(result, canAssignToVariable);

			return result;
		}

		private bool TryValue(out ExpressionResult result)
		{
			return TryParseWithPrecedence(Precedence.Primary, out result);
		}

		private void IfStatement()
		{
			var expression = Expression(false);
			if (expression.valueCount != 1)
			{
				compiler.AddSoftError(expression.slice, new ExprectedOneValueAsIfConditionError
				{
					got = expression.valueCount
				});
			}

			var elseJump = compiler.BeginEmitForwardJump(Instruction.PopAndJumpForwardIfFalse);

			compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new ExpectedOpenCurlyBracesAfterIfConditionError());
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

		private void IterateStatement()
		{
			// BACKWARD JUMP

			var expression = Expression(false);
			if (expression.valueCount != 2)
			{
				compiler.AddSoftError(expression.slice, new ExpectedTwoValuesAsIterateConditionError
				{
					got = expression.valueCount
				});
			}

			compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new ExpectedOpenCurlyBracesAfterIterateConditionError());
			Block();

			// BACKWARD JUMP
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

		internal static byte Grouping(CompilerController self)
		{
			var expression = self.Expression(false);
			self.compiler.parser.Consume(TokenKind.CloseParenthesis, new ExpectedCloseParenthesisAfterExpressionError());
			return expression.valueCount;
		}

		private ExpressionResult Pipe(ExpressionResult previous, bool canAssignToVariable)
		{
			var slice = compiler.parser.previousToken.slice;

			while (!compiler.parser.Check(TokenKind.End))
			{
				compiler.parser.Next();
				switch (compiler.parser.previousToken.kind)
				{
				case TokenKind.Variable:
					AssignLocals(canAssignToVariable, previous.valueCount);
					slice = Slice.FromTo(slice, compiler.parser.previousToken.slice);
					return new ExpressionResult(slice, 0);
				case TokenKind.Identifier:
					previous = PipeCommand(previous);
					break;
				default:
					compiler.AddHardError(compiler.parser.previousToken.slice, new InvalidTokenAfterPipeError());
					slice = Slice.FromTo(slice, compiler.parser.previousToken.slice);
					return new ExpressionResult(slice, 0);
				}

				if (!compiler.parser.Match(TokenKind.Pipe))
					break;
			}

			return previous;
		}

		internal static byte Comma(CompilerController self, ExpressionResult previous)
		{
			var expression = self.Expression(false);
			var valueCount = previous.valueCount + expression.valueCount;

			if (valueCount > byte.MaxValue)
			{
				var slice = Slice.FromTo(previous.slice, expression.slice);
				self.compiler.AddSoftError(slice, new TooManyExpressionValuesError());
				return byte.MaxValue;
			}

			return (byte)valueCount;
		}

		internal static byte Command(CompilerController self)
		{
			var result = new ExpressionResult(self.compiler.parser.previousToken.slice, 0);
			return self.PipeCommand(result).valueCount;
		}

		private ExpressionResult PipeCommand(ExpressionResult previous)
		{
			var commandSlice = compiler.parser.previousToken.slice;
			var slice = commandSlice;

			var argCount = 0;
			while (TryValue(out var valueResult))
			{
				slice = Slice.FromTo(slice, valueResult.slice);
				argCount += valueResult.valueCount;
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
				return new ExpressionResult(slice, 0);
			}
			else
			{
				var command = compiler.chunk.commandDefinitions.buffer[commandIndex];
				if (argCount != command.parameterCount)
				{
					compiler.AddSoftError(slice, new WrongNumberOfCommandArgumentsError
					{
						commandName = command.name,
						expected = command.parameterCount,
						got = argCount
					});
				}
				else
				{
					compiler.EmitCallNativeCommand(commandIndex, previous.valueCount);
				}

				return new ExpressionResult(slice, command.returnCount);
			}
		}

		private void AssignLocals(bool canAssign, byte valueCount)
		{
			var slice = compiler.parser.previousToken.slice;

			if (!canAssign)
			{
				compiler.AddSoftError(slice, new CanOnlyAssignVariablesAtTopLevelExpressionsError());
				return;
			}

			if (compiler.ResolveToLocalVariableIndex(slice, out var localIndex))
			{
				if (valueCount != 1)
				{
					compiler.AddSoftError(slice, new ExpectedOneValueToAssignToVariableError
					{
						variableName = CompilerHelper.GetSlice(compiler, slice),
						got = valueCount
					});
				}

				compiler.EmitInstruction(Instruction.AssignLocal);
				compiler.EmitByte(localIndex);
			}
			else
			{
				if (compiler.localVariables.count >= byte.MaxValue)
					compiler.AddSoftError(slice, new TooManyVariablesError());

				compiler.AddLocalVariable(slice, false);
			}
		}

		internal static byte LoadLocal(CompilerController self)
		{
			var slice = self.compiler.parser.previousToken.slice;

			if (self.compiler.ResolveToLocalVariableIndex(slice, out var localIndex))
			{
				self.compiler.localVariables.buffer[localIndex].used = true;
				self.compiler.EmitInstruction(Instruction.LoadLocal);
				self.compiler.EmitByte(localIndex);
			}
			else
			{
				self.compiler.AddSoftError(slice, new LocalVariableUnassignedError { name = CompilerHelper.GetSlice(self.compiler, slice) });
			}

			return 1;
		}

		internal static byte Literal(CompilerController self)
		{
			switch (self.compiler.parser.previousToken.kind)
			{
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

			return 1;
		}
	}
}