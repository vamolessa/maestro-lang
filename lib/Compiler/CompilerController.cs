namespace Maestro
{
	internal sealed class CompilerController
	{
		public readonly Compiler compiler = new Compiler();
		public Buffer<Source> compiledSources = new Buffer<Source>(1);

		private readonly ParseRules parseRules = new ParseRules();
		private Option<IImportResolver> importResolver = Option.None;
		private Buffer<Slice> slicesCache = new Buffer<Slice>(1);

		public Buffer<CompileError> CompileSource(ByteCodeChunk chunk, Option<IImportResolver> importResolver, Mode mode, Source source)
		{
			this.importResolver = importResolver;
			compiledSources.ZeroClear();

			compiler.Reset(mode, chunk);
			Compile(source);

			return compiler.errors;
		}

		private void Compile(Source source)
		{
			compiler.BeginSource(source.content, compiledSources.count);
			compiledSources.PushBack(source);

			compiler.parser.Next();
			while (!compiler.parser.Match(TokenKind.End))
				Declaration();

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

		private void Declaration()
		{
			if (compiler.parser.Match(TokenKind.Import))
				ImportDeclaration();
			else if (compiler.parser.Match(TokenKind.External))
				ExternalCommandDeclaration();
			else if (compiler.parser.Match(TokenKind.Command))
				CommandDeclaration();
			else
				Statement();
		}

		private void ImportDeclaration()
		{
			var slice = compiler.parser.previousToken.slice;
			compiler.parser.Consume(TokenKind.StringLiteral, new CompileErrors.Imports.ExpectedImportPathString());
			var modulePath = CompilerHelper.GetParsedString(compiler);
			slice = slice.ExpandedTo(compiler.parser.previousToken.slice);
			CompilerHelper.ConsumeSemicolon(compiler, slice, new CompileErrors.Imports.ExpectedSemiColonAfterImport());
			slice = slice.ExpandedTo(compiler.parser.previousToken.slice);

			if (!importResolver.isSome)
			{
				compiler.AddSoftError(slice, new CompileErrors.Imports.NoImportResolverProvided { uri = modulePath });
				return;
			}

			var currentSource = compiledSources.buffer[compiledSources.count - 1];
			var moduleUri = Uri.Resolve(currentSource.uri, modulePath);

			for (var i = 0; i < compiledSources.count; i++)
			{
				if (compiledSources.buffer[i].uri.value == moduleUri.value)
					return;
			}

			var moduleSource = importResolver.value.ResolveSource(currentSource.uri, moduleUri);
			if (!moduleSource.isSome)
			{
				compiler.AddSoftError(slice, new CompileErrors.Imports.CouldNotResolveImport { importUri = moduleUri.value, fromUri = currentSource.uri.value });
				return;
			}

			Compile(new Source(moduleUri, moduleSource.value));
		}

		private void Statement()
		{
			if (compiler.parser.Match(TokenKind.If))
				IfStatement();
			else if (compiler.parser.Match(TokenKind.Iterate))
				IterateStatement();
			else if (compiler.parser.Match(TokenKind.Return))
				ReturnStatement();
			else
				ExpressionStatement();
		}

		private void ExternalCommandDeclaration()
		{
			var slice = compiler.parser.previousToken.slice;

			compiler.parser.Consume(TokenKind.Command, new CompileErrors.ExternalCommands.ExpectedCommandKeyword());
			compiler.parser.Consume(TokenKind.Identifier, new CompileErrors.ExternalCommands.ExpectedExternalCommandIdentifier());
			var nameSlice = compiler.parser.previousToken.slice;
			var name = CompilerHelper.GetSlice(compiler, nameSlice);

			compiler.parser.Consume(TokenKind.IntLiteral, new CompileErrors.ExternalCommands.ExpectedExternalCommandParameterCount());
			var parameterCount = CompilerHelper.GetParsedInt(compiler);
			if (parameterCount > byte.MaxValue)
			{
				compiler.AddSoftError(compiler.parser.previousToken.slice, new CompileErrors.ExternalCommands.TooManyExternalCommandParameters());
				parameterCount = byte.MaxValue;
			}

			slice = slice.ExpandedTo(compiler.parser.previousToken.slice);
			CompilerHelper.ConsumeSemicolon(compiler, slice, new CompileErrors.ExternalCommands.ExpectedSemiColonAfterExternCommand());

			var success = compiler.chunk.AddExternalCommand(new ExternalCommandDefinition(name, (byte)parameterCount));
			if (!success)
				compiler.AddSoftError(nameSlice, new CompileErrors.Commands.CommandNameAlreadyRegistered { name = name });
		}

		private void CommandDeclaration()
		{
			var skipJump = compiler.BeginEmitForwardJump(Instruction.JumpForward);
			var commandCodeIndex = compiler.chunk.bytes.count;

			compiler.parser.Consume(TokenKind.Identifier, new CompileErrors.Commands.ExpectedCommandIdentifier());
			var nameSlice = compiler.parser.previousToken.slice;
			var name = CompilerHelper.GetSlice(compiler, nameSlice);

			compiler.BeginScope(ScopeType.CommandBody);

			var parameterCount = 0;
			var parametersSlice = compiler.parser.currentToken.slice;
			while (
				!compiler.parser.Check(TokenKind.End) &&
				!compiler.parser.Check(TokenKind.OpenCurlyBrackets)
			)
			{
				compiler.parser.Consume(TokenKind.Variable, new CompileErrors.Commands.ExpectedCommandParameterVariable());
				var parameterSlice = compiler.parser.previousToken.slice;
				parametersSlice = parametersSlice.ExpandedTo(parameterSlice);
				parameterCount += 1;

				compiler.AddLocalVariable(parameterSlice, LocalVariableFlag.NotRead);
			}

			if (parameterCount > byte.MaxValue)
			{
				compiler.AddSoftError(parametersSlice, new CompileErrors.Commands.TooManyExternalCommandParameterVariables());
				parameterCount = byte.MaxValue;
			}

			compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new CompileErrors.Commands.ExpectedOpenCurlyBracesBeforeCommandBody());
			Block();

			compiler.EndScope();

			compiler.EmitInstruction(Instruction.PushEmptyTuple);
			compiler.EmitInstruction(Instruction.Return);
			compiler.EndEmitForwardJump(skipJump);

			var success = compiler.chunk.AddCommand(new CommandDefinition(name, commandCodeIndex, (byte)parameterCount));
			if (!success)
				compiler.AddSoftError(nameSlice, new CompileErrors.Commands.CommandNameAlreadyRegistered { name = name });
		}

		private void IfStatement()
		{
			Expression(false);
			var elseJump = compiler.BeginEmitForwardJump(Instruction.PopExpressionAndJumpForwardIfFalse);

			compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new CompileErrors.If.ExpectedOpenCurlyBracesAfterIfCondition());
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
					compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new CompileErrors.If.ExpectedOpenCurlyBracesAfterElse());
					Block();
				}
			}

			compiler.EndEmitForwardJump(thenJump);
		}

		private void IterateStatement()
		{
			var loopJump = compiler.BeginEmitBackwardJump();

			compiler.BeginScope(ScopeType.IterationBody);
			Expression(false);

			var breakJump = compiler.BeginEmitForwardJump(Instruction.JumpForwardIfExpressionIsEmptyKeepingOne);

			compiler.AddLocalVariable(default, LocalVariableFlag.Input);

			compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new CompileErrors.Iterate.ExpectedOpenCurlyBracesAfterIterateCondition());
			Block();

			compiler.EndScope();
			compiler.EndEmitBackwardJump(Instruction.JumpBackward, loopJump);
			compiler.EndEmitForwardJump(breakJump);
			compiler.EmitKeep(0);
		}

		private void ReturnStatement()
		{
			var slice = compiler.parser.previousToken.slice;

			if (compiler.parser.Check(TokenKind.SemiColon))
				compiler.EmitInstruction(Instruction.PushEmptyTuple);
			else
				Expression(false);

			slice = slice.ExpandedTo(compiler.parser.previousToken.slice);
			CompilerHelper.ConsumeSemicolon(compiler, slice, new CompileErrors.Commands.ExpectedSemiColonAfterReturn());

			compiler.EmitInstruction(Instruction.Return);
		}

		private void ExpressionStatement()
		{
			(var slice, var assignedToVariable) = Expression(true);

			CompilerHelper.ConsumeSemicolon(compiler, slice, new CompileErrors.General.ExpectedSemiColonAfterExpression());

			if (!assignedToVariable)
				compiler.EmitKeep(0);
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
				slice = slice.ExpandedTo(parser.previousToken.slice);
			}

			slice = slice.ExpandedTo(parser.previousToken.slice);
			return true;
		}

		private Slice ParseWithPrecedence(Precedence precedence)
		{
			if (!TryParseWithPrecedence(precedence, out var slice))
			{
				compiler.AddHardError(slice, new CompileErrors.General.ExpectedExpression());
			}

			return slice;
		}

		private (Slice, bool) Expression(bool canAssignToVariable)
		{
			var slice = ParseWithPrecedence(Precedence.Expression);

			var assignedToVariable = false;
			if (compiler.parser.Match(TokenKind.Pipe))
			{
				assignedToVariable = Pipe(canAssignToVariable);
				slice = slice.ExpandedTo(compiler.parser.previousToken.slice);
			}

			return (slice, assignedToVariable);
		}

		private bool TryValue(out Slice slice)
		{
			return TryParseWithPrecedence(Precedence.Primary, out slice);
		}

		private void Block()
		{
			compiler.BeginScope(ScopeType.Normal);
			while (
				!compiler.parser.Check(TokenKind.End) &&
				!compiler.parser.Check(TokenKind.CloseCurlyBrackets)
			)
			{
				Statement();
			}

			compiler.parser.Consume(TokenKind.CloseCurlyBrackets, new CompileErrors.Block.ExpectedCloseCurlyBracketsAfterBlock());
			compiler.EndScope();
		}

		internal static void Group(CompilerController self)
		{
			self.Expression(false);
			self.compiler.parser.Consume(TokenKind.CloseParenthesis, new CompileErrors.Group.ExpectedCloseParenthesisAfterExpression());
		}

		private bool Pipe(bool canAssignToVariable)
		{
			while (!compiler.parser.Check(TokenKind.End))
			{
				compiler.parser.Next();
				switch (compiler.parser.previousToken.kind)
				{
				case TokenKind.Variable:
					AssignLocals(canAssignToVariable);
					return true;
				case TokenKind.Identifier:
					PipedCommand();
					break;
				default:
					compiler.AddHardError(compiler.parser.previousToken.slice, new CompileErrors.Pipe.InvalidTokenAfterPipe());
					return true;
				}

				if (!compiler.parser.Match(TokenKind.Pipe))
					break;
			}

			return false;
		}

		internal static void Comma(CompilerController self, Slice previous)
		{
			var expressionSlice = self.ParseWithPrecedence(Precedence.Comma + 1);
			self.compiler.EmitInstruction(Instruction.MergeTuple);
		}

		internal static void Command(CompilerController self)
		{
			self.compiler.EmitInstruction(Instruction.PushEmptyTuple);
			self.PipedCommand();
		}

		private void PipedCommand()
		{
			var commandSlice = compiler.parser.previousToken.slice;
			var slice = commandSlice;

			var argCount = 0;
			while (TryValue(out var valueSlice))
			{
				slice = slice.ExpandedTo(valueSlice);
				compiler.EmitKeep(1);
				argCount += 1;
			}

			if (compiler.ResolveToExternalCommandIndex(commandSlice).TryGet(out var externalCommandIndex))
			{
				var externalCommand = compiler.chunk.externalCommandDefinitions.buffer[externalCommandIndex];
				if (argCount != externalCommand.parameterCount)
				{
					compiler.AddSoftError(slice, new CompileErrors.ExternalCommands.WrongNumberOfExternalCommandArguments
					{
						commandName = externalCommand.name,
						expected = externalCommand.parameterCount,
						got = argCount
					});
				}
				else
				{
					compiler.EmitExecuteNativeCommand(externalCommandIndex);
				}
			}
			else if (compiler.ResolveToCommandIndex(commandSlice).TryGet(out var commandIndex))
			{
				var command = compiler.chunk.commandDefinitions.buffer[commandIndex];
				if (argCount != command.parameterCount)
				{
					compiler.AddSoftError(slice, new CompileErrors.Commands.WrongNumberOfCommandArguments
					{
						commandName = command.name,
						expected = command.parameterCount,
						got = argCount
					});
				}
				else
				{
					compiler.EmitExecuteCommand(commandIndex);
				}
			}
			else
			{
				compiler.AddSoftError(slice, new CompileErrors.Commands.CommandNotRegistered { name = CompilerHelper.GetSlice(compiler, commandSlice) });
			}
		}

		private void AssignLocals(bool canAssign)
		{
			var slice = compiler.parser.previousToken.slice;

			slicesCache.count = 0;
			slicesCache.PushBackUnchecked(slice);

			var isAssignment = compiler.ResolveToLocalVariableIndex(slice).isSome;
			var hasMixedAssignmentType = false;

			while (compiler.parser.Match(TokenKind.Comma))
			{
				compiler.parser.Consume(TokenKind.Variable, new CompileErrors.Variables.ExpectedVariableAsAssignmentTarget());

				var varSlice = compiler.parser.previousToken.slice;
				slicesCache.PushBackUnchecked(varSlice);

				var isAnotherAssignment = compiler.ResolveToLocalVariableIndex(varSlice).isSome;
				if (isAssignment != isAnotherAssignment)
					hasMixedAssignmentType = true;
			}

			slice = slicesCache.buffer[0].ExpandedTo(compiler.parser.previousToken.slice);

			if (!canAssign)
			{
				compiler.AddSoftError(slice, new CompileErrors.Variables.CanOnlyAssignToVariablesAtTopLevelExpressions());
			}
			else if (hasMixedAssignmentType)
			{
				compiler.AddSoftError(slice, new CompileErrors.Variables.MixedAssignmentType());
			}
			else if (slicesCache.count > byte.MaxValue)
			{
				compiler.AddSoftError(slice, new CompileErrors.Variables.TooManyVariablesOnAssignment());
			}
			else if (isAssignment)
			{
				compiler.EmitKeep((byte)slicesCache.count);

				for (var i = slicesCache.count - 1; i >= 0; i--)
				{
					var varSlice = slicesCache.buffer[i];
					var localIndex = compiler.ResolveToLocalVariableIndex(varSlice).value;
					compiler.EmitLocalInstruction(Instruction.AssignLocal, localIndex);

					compiler.localVariables.buffer[localIndex].PerformedWrite();
				}
			}
			else
			{
				compiler.EmitKeep((byte)slicesCache.count);

				for (var i = 0; i < slicesCache.count; i++)
				{
					var varSlice = slicesCache.buffer[i];
					if (compiler.localVariables.count >= byte.MaxValue)
						compiler.AddSoftError(varSlice, new CompileErrors.Variables.TooManyVariables());
					compiler.AddLocalVariable(varSlice, LocalVariableFlag.NotRead);
				}
			}
		}

		internal static void LoadLocal(CompilerController self)
		{
			var slice = self.compiler.parser.previousToken.slice;

			if (self.compiler.ResolveToLocalVariableIndex(slice).TryGet(out var localIndex))
			{
				ref var variable = ref self.compiler.localVariables.buffer[localIndex];
				variable.PerformedRead();

				if (variable.flag == LocalVariableFlag.Unwritten)
				{
					self.compiler.AddSoftError(slice, new CompileErrors.Variables.LocalVariableUnassigned { name = CompilerHelper.GetSlice(self.compiler, slice) });
				}

				self.compiler.EmitLocalInstruction(Instruction.LoadLocal, localIndex);
			}
			else
			{
				self.compiler.AddSoftError(slice, new CompileErrors.Variables.LocalVariableUnassigned { name = CompilerHelper.GetSlice(self.compiler, slice) });
			}
		}

		internal static void LoadInput(CompilerController self)
		{
			LoadLocal(self);
		}

		internal static void Literal(CompilerController self)
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
				self.compiler.AddHardError(self.compiler.parser.previousToken.slice, new CompileErrors.Literals.ExpectedLiteral { got = self.compiler.parser.previousToken.kind });
				break;
			}
		}
	}
}