namespace Maestro
{
	internal sealed class CompilerController
	{
		internal readonly Compiler compiler = new Compiler();
		internal readonly ParseRules parseRules = new ParseRules();

		internal Buffer<Slice> slicesCache = new Buffer<Slice>(1);
		internal AssemblyRegistry assemblyRegistry;
		internal NativeCommandBindingRegistry bindingRegistry;

		internal Buffer<CompileError> CompileSource(Mode mode, Source source, AssemblyRegistry assemblyRegistry, NativeCommandBindingRegistry bindingRegistry, out Assembly assembly)
		{
			this.assemblyRegistry = assemblyRegistry;
			this.bindingRegistry = bindingRegistry;

			assembly = new Assembly(source);
			assembly.AddCommand(new CommandDefinition("entry point", 0, new Slice(), 0));
			compiler.Reset(assembly, mode, source);

			compiler.BeginScope(ScopeType.Normal);

			compiler.parser.Next();
			while (!compiler.parser.Match(TokenKind.End))
				Declaration();

			compiler.EndScope();
			compiler.EmitInstruction(Instruction.PushEmptyTuple);
			compiler.EmitInstruction(Instruction.Return);
			compiler.EmitInstruction(Instruction.Halt);

			return compiler.errors;
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
			if (compiler.parser.Match(TokenKind.Command))
				CommandDeclaration();
			else
				Statement();
		}

		private void Statement()
		{
			if (compiler.parser.Match(TokenKind.If))
				IfStatement();
			else if (compiler.parser.Match(TokenKind.ForEach))
				ForEachStatement();
			else if (compiler.parser.Match(TokenKind.Return))
				ReturnStatement();
			else
				ExpressionStatement();
		}

		private void CommandDeclaration()
		{
			var skipJump = compiler.BeginEmitForwardJump(Instruction.JumpForward);
			var commandCodeIndex = compiler.assembly.bytes.count;

			compiler.parser.Consume(TokenKind.Identifier, new CompileErrors.Commands.ExpectedCommandIdentifier());
			var nameSlice = compiler.parser.previousToken.slice;
			var name = CompilerHelper.GetSlice(compiler, nameSlice);

			compiler.EmitDebugInstruction(Instruction.DebugPushDebugFrame);
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

				for (var i = 0; i < parameterCount; i++)
				{
					ref var otherParameter = ref compiler.variables.buffer[compiler.variables.count - parameterCount + i];
					var otherParameterSlice = otherParameter.slice;
					if (CompilerHelper.AreEqual(compiler.parser.tokenizer.source, parameterSlice, otherParameterSlice))
					{
						compiler.AddSoftError(parameterSlice, new CompileErrors.Commands.DuplicatedCommandParameterVariable
						{
							commandName = name,
							parameterName = CompilerHelper.GetSlice(compiler, parameterSlice)
						});
						otherParameter.flag = VariableFlag.Fulfilled;
						break;
					}
				}

				parametersSlice = parametersSlice.ExpandedTo(parameterSlice);
				parameterCount += 1;

				compiler.AddVariable(parameterSlice, VariableFlag.NotRead);
			}

			if (parameterCount > byte.MaxValue)
			{
				compiler.AddSoftError(parametersSlice, new CompileErrors.Commands.TooManyNativeCommandParameterVariables());
				parameterCount = byte.MaxValue;
			}

			var nativeCommandInstancesBaseIndex = compiler.assembly.nativeCommandInstances.count;

			compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new CompileErrors.Commands.ExpectedOpenCurlyBracesBeforeCommandBody());
			Block();

			compiler.EndScope();
			compiler.EmitDebugInstruction(Instruction.DebugPopDebugFrame);

			compiler.EmitInstruction(Instruction.PushEmptyTuple);
			compiler.EmitInstruction(Instruction.Return);
			compiler.EndEmitForwardJump(skipJump);

			var success = compiler.assembly.AddCommand(new CommandDefinition(
				name,
				commandCodeIndex,
				new Slice(
					nativeCommandInstancesBaseIndex,
					compiler.assembly.nativeCommandInstances.count - nativeCommandInstancesBaseIndex
				),
				(byte)parameterCount
			));

			if (!success)
				compiler.AddSoftError(nameSlice, new CompileErrors.Commands.CommandNameDuplicated { name = name });

			if (compiler.assembly.commandDefinitions.count > byte.MaxValue)
				compiler.AddSoftError(nameSlice, new CompileErrors.Commands.TooManyCommandsDefined());
		}

		private void IfStatement()
		{
			Expression(false);
			var elseJump = compiler.BeginEmitForwardJump(Instruction.IfConditionJump);

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

		private void ForEachStatement()
		{
			compiler.parser.Consume(TokenKind.Variable, new CompileErrors.ForEach.ExpectedForEachVariable());
			var elementVariableSlice = compiler.parser.previousToken.slice;

			compiler.parser.Consume(TokenKind.In, new CompileErrors.ForEach.ExpectedInAfterForEachVariable());

			compiler.EmitPushLiteral(new Value(-1));
			compiler.EmitPushLiteral(default);
			compiler.EmitKeep(1);
			compiler.EmitKeep(1);

			Expression(false);

			compiler.BeginScope(ScopeType.IterationBody);
			compiler.AddVariable(new Slice(), VariableFlag.Fulfilled);
			compiler.AddVariable(elementVariableSlice, VariableFlag.NotRead);

			var loopJump = compiler.BeginEmitBackwardJump();
			var breakJump = compiler.BeginEmitForwardJump(Instruction.ForEachConditionJump);

			compiler.parser.Consume(TokenKind.OpenCurlyBrackets, new CompileErrors.ForEach.ExpectedOpenCurlyBracesAfterForEachExpression());
			Block();

			compiler.EndEmitBackwardJump(Instruction.JumpBackward, loopJump);
			compiler.EndEmitForwardJump(breakJump);
			compiler.EndScope();
		}

		private void ReturnStatement()
		{
			var slice = compiler.parser.previousToken.slice;
			if (compiler.parser.Check(TokenKind.SemiColon))
				compiler.EmitInstruction(Instruction.PushEmptyTuple);
			else
				Expression(false);

			slice = slice.ExpandedTo(compiler.parser.previousToken.slice);
			CompilerHelper.ConsumeSemicolon(compiler, slice, new CompileErrors.Return.ExpectedSemiColonAfterReturn());

			if (compiler.GetTopCommandScope().isSome)
				compiler.EmitDebugInstruction(Instruction.DebugPopDebugFrame);

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
				return false;
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

			if (compiler.ResolveToNativeCommandIndex(bindingRegistry, commandSlice).TryGet(out var nativeCommandIndex))
			{
				var nativeCommand = compiler.assembly.dependencyNativeCommandDefinitions.buffer[nativeCommandIndex];
				if (argCount != nativeCommand.parameterCount)
				{
					compiler.AddSoftError(slice, new CompileErrors.NativeCommands.WrongNumberOfNativeCommandArguments
					{
						commandName = nativeCommand.name,
						expected = nativeCommand.parameterCount,
						got = argCount
					});
				}
				else
				{
					compiler.EmitExecuteNativeCommand(nativeCommandIndex, slice);
				}
			}
			else if (compiler.ResolveToCommandIndex(compiler.assembly, commandSlice).TryGet(out var commandIndex))
			{
				var command = compiler.assembly.commandDefinitions.buffer[commandIndex];
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
			else if (compiler.ResolveToExternalCommandIndex(assemblyRegistry, commandSlice).TryGet(out var externalCommandReference))
			{
				var command = externalCommandReference.assembly.commandDefinitions.buffer[externalCommandReference.commandIndex];
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

			var isAssignment = compiler.ResolveToVariableIndex(slice).isSome;
			var hasMixedAssignmentType = false;

			while (compiler.parser.Match(TokenKind.Comma))
			{
				compiler.parser.Consume(TokenKind.Variable, new CompileErrors.Variables.ExpectedVariableAsAssignmentTarget());

				var varSlice = compiler.parser.previousToken.slice;
				slicesCache.PushBackUnchecked(varSlice);

				var isAnotherAssignment = compiler.ResolveToVariableIndex(varSlice).isSome;
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
					var variableIndex = compiler.ResolveToVariableIndex(varSlice).value;
					compiler.EmitVariableInstruction(Instruction.SetLocal, variableIndex);

					compiler.variables.buffer[variableIndex].PerformedWrite();
				}
			}
			else
			{
				compiler.EmitKeep((byte)slicesCache.count);

				for (var i = 0; i < slicesCache.count; i++)
				{
					var varSlice = slicesCache.buffer[i];
					if (compiler.variables.count >= byte.MaxValue)
						compiler.AddSoftError(varSlice, new CompileErrors.Variables.TooManyVariables());
					compiler.AddVariable(varSlice, VariableFlag.NotRead);
				}
			}
		}

		internal static void LoadLocal(CompilerController self)
		{
			var slice = self.compiler.parser.previousToken.slice;

			if (self.compiler.ResolveToVariableIndex(slice).TryGet(out var variableIndex))
			{
				ref var variable = ref self.compiler.variables.buffer[variableIndex];
				variable.PerformedRead();

				if (variable.flag == VariableFlag.Unwritten)
				{
					self.compiler.AddSoftError(slice, new CompileErrors.Variables.VariableUnassigned { name = CompilerHelper.GetSlice(self.compiler, slice) });
				}

				self.compiler.EmitVariableInstruction(Instruction.PushLocal, variableIndex);
			}
			else
			{
				self.compiler.AddSoftError(slice, new CompileErrors.Variables.VariableUnassigned { name = CompilerHelper.GetSlice(self.compiler, slice) });
			}
		}

		internal static void LoadInput(CompilerController self)
		{
			self.compiler.EmitInstruction(Instruction.PushInput);
		}

		internal static void Literal(CompilerController self)
		{
			switch (self.compiler.parser.previousToken.kind)
			{
			case TokenKind.False:
				self.compiler.EmitInstruction(Instruction.PushFalse);
				break;
			case TokenKind.True:
				self.compiler.EmitInstruction(Instruction.PushTrue);
				break;
			case TokenKind.IntLiteral:
				self.compiler.EmitPushLiteral(new Value(CompilerHelper.GetParsedInt(self.compiler)));
				break;
			case TokenKind.FloatLiteral:
				self.compiler.EmitPushLiteral(new Value(CompilerHelper.GetParsedFloat(self.compiler)));
				break;
			case TokenKind.StringLiteral:
				self.compiler.EmitPushLiteral(new Value(CompilerHelper.GetParsedString(self.compiler)));
				break;
			default:
				self.compiler.AddHardError(self.compiler.parser.previousToken.slice, new CompileErrors.Literals.ExpectedLiteral { got = self.compiler.parser.previousToken.kind });
				break;
			}
		}
	}
}