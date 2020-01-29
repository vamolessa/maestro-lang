namespace Flow
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

		public static void EmitPop(this Compiler self, int count)
		{
			if (count > 1)
			{
				self.EmitInstruction(Instruction.PopMultiple);

				if (count > byte.MaxValue)
				{
					self.EmitByte(byte.MaxValue);
					EmitPop(self, count - byte.MaxValue);
				}
				else
				{
					self.EmitByte((byte)count);
				}
			}
			else if (count > 0)
			{
				self.EmitInstruction(Instruction.Pop);
			}
		}

		public static void EmitLoadLiteral(this Compiler self, Value value)
		{
			var index = self.chunk.AddLiteral(value);
			self.EmitInstruction(Instruction.LoadLiteral);
			self.EmitUShort((ushort)index);
		}

		public static void EmitCallNativeCommand(this Compiler self, int commandIndex, byte inputCount)
		{
			var instanceIndex = self.chunk.commandInstances.count;
			self.chunk.commandInstances.PushBack(commandIndex);
			self.EmitInstruction(Instruction.ExecuteNativeCommand);
			self.EmitUShort((ushort)instanceIndex);
			self.EmitByte(inputCount);
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
	}
}