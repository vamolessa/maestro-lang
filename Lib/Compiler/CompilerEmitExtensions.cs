namespace Maestro
{
	internal static class CompilerEmitExtensions
	{
		public static void EmitByte(this Compiler self, byte value)
		{
			self.chunk.WriteByte(value, self.parser.previousToken.slice);
		}

		public static void EmitUShort(this Compiler self, ushort value)
		{
			BytesHelper.UShortToBytes(value, out var b0, out var b1);
			self.chunk.WriteByte(b0, self.parser.previousToken.slice);
			self.chunk.WriteByte(b1, self.parser.previousToken.slice);
		}

		public static void EmitInstruction(this Compiler self, Instruction instruction)
		{
			if (self.mode == Mode.Debug && instruction < Instruction.DebugHook)
				self.EmitByte((byte)Instruction.DebugHook);
			self.EmitByte((byte)instruction);
		}

		public static void EmitKeep(this Compiler self, byte count)
		{
			self.EmitInstruction(Instruction.PopTupleKeeping);
			self.EmitByte(count);
		}

		public static void EmitPop(this Compiler self, byte count)
		{
			if (count > 0)
			{
				self.EmitInstruction(Instruction.Pop);
				self.EmitByte(count);
			}
		}

		public static void EmitPushLiteral(this Compiler self, Value value)
		{
			var index = self.chunk.AddLiteral(value);
			self.EmitInstruction(Instruction.PushLiteral);
			self.EmitUShort((ushort)index);
		}

		public static void EmitVariableInstruction(this Compiler self, Instruction instruction, int variableIndex)
		{
			var commandVariablesBaseIndex = self.GetTopCommandScope().Select(s => s.variablesStartIndex).GetOr(0);

			var localIndex = variableIndex - commandVariablesBaseIndex;
			if (localIndex < 0)
				return;

			self.EmitInstruction(instruction);
			self.EmitByte((byte)localIndex);
		}

		public static void EmitExecuteExternalCommand(this Compiler self, int commandIndex, Slice slice)
		{
			var instanceIndex = self.chunk.externalCommandInstances.count;
			self.chunk.externalCommandInstances.PushBack(new ExternalCommandInstance(commandIndex, self.sourceIndex, slice));
			self.EmitInstruction(Instruction.ExecuteNativeCommand);
			self.EmitUShort((ushort)instanceIndex);
		}

		public static void EmitExecuteCommand(this Compiler self, int commandIndex)
		{
			self.EmitInstruction(Instruction.ExecuteCommand);
			self.EmitUShort((ushort)commandIndex);
		}

		public static int BeginEmitBackwardJump(this Compiler self)
		{
			return self.chunk.bytes.count;
		}

		public static void EndEmitBackwardJump(this Compiler self, Instruction instruction, int jumpIndex)
		{
			self.EmitInstruction(instruction);

			var offset = self.chunk.bytes.count - jumpIndex + 2;
			if (offset > ushort.MaxValue)
			{
				self.AddSoftError(self.parser.previousToken.slice, new CompileErrors.General.TooMuchCodeToJumpOver());
				offset = 0;
			}

			self.EmitUShort((ushort)offset);
		}

		public static int BeginEmitForwardJump(this Compiler self, Instruction instruction)
		{
			self.EmitInstruction(instruction);
			self.EmitUShort(0);

			return self.chunk.bytes.count - 2;
		}

		public static void EndEmitForwardJump(this Compiler self, int jumpIndex)
		{
			var offset = self.chunk.bytes.count - jumpIndex - 2;
			if (offset > ushort.MaxValue)
			{
				self.AddSoftError(self.parser.previousToken.slice, new CompileErrors.General.TooMuchCodeToJumpOver());
				offset = 0;
			}

			BytesHelper.UShortToBytes(
				(ushort)offset,
				out self.chunk.bytes.buffer[jumpIndex],
				out self.chunk.bytes.buffer[jumpIndex + 1]
			);
		}

		public static void EmitDebugInstruction(this Compiler self, Instruction instruction)
		{
			if (self.mode == Mode.Debug)
				self.EmitInstruction(instruction);
		}

		public static void EmitDebugPushVariableInfo(this Compiler self, Slice slice)
		{
			if (self.mode != Mode.Debug)
				return;

			var name = CompilerHelper.GetSlice(self, slice);
			var nameLiteralIndex = self.chunk.AddLiteral(new Value(name));

			var variablesStartIndex = self.GetTopCommandScope().Select(s => s.variablesStartIndex).GetOr(0);
			var stackIndex = self.variables.count - variablesStartIndex;
			if (stackIndex > byte.MaxValue)
				stackIndex = byte.MaxValue;

			self.EmitInstruction(Instruction.DebugPushVariableInfo);
			self.EmitUShort((ushort)nameLiteralIndex);
			self.EmitByte((byte)stackIndex);
		}

		public static void EmitDebugPopVariableInfo(this Compiler self, int count)
		{
			if (self.mode != Mode.Debug)
				return;

			if (count == 0)
				return;

			if (count > byte.MaxValue)
				count = byte.MaxValue;

			self.EmitInstruction(Instruction.DebugPopVariableInfo);
			self.EmitByte((byte)count);
		}
	}
}