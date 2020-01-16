namespace Flow
{
	internal static class CompilerEmitExtensions
	{
		public static void EmitByte(this CompilerIO self, byte value)
		{
			self.chunk.WriteByte(value, self.parser.previousToken.slice);
		}

		public static void EmitUShort(this CompilerIO self, ushort value)
		{
			BytesHelper.UShortToBytes(value, out var b0, out var b1);
			self.chunk.WriteByte(b0, self.parser.previousToken.slice);
			self.chunk.WriteByte(b1, self.parser.previousToken.slice);
		}

		public static void EmitInstruction(this CompilerIO self, Instruction instruction)
		{
			self.EmitByte((byte)instruction);
		}

		public static void EmitLoadLiteral(this CompilerIO self, object value)
		{
			var index = self.chunk.AddLiteral(value);
			self.EmitInstruction(Instruction.LoadLiteral);
			self.EmitUShort((ushort)index);
		}

		public static void EmitVariableInstruction(this CompilerIO self, Instruction instruction, string variableName)
		{
			var index = self.chunk.AddVariableName(variableName);
			self.EmitInstruction(instruction);
			self.EmitUShort((ushort)index);
		}

		public static void EmitRunCommandInstance(this CompilerIO self, int commandIndex, byte argCount)
		{
			var instanceIndex = self.chunk.commandInstances.count;
			self.chunk.commandInstances.PushBack(commandIndex);
			self.EmitInstruction(Instruction.RunCommandInstance);
			self.EmitUShort((ushort)instanceIndex);
			self.EmitByte(argCount);
		}

		/*
		public static void EmitSetLocal(this CompilerIO self, int stackIndex, ValueType type)
		{
			var typeSize = type.GetSize(self.chunk);
			if (typeSize > 1)
			{
				self.EmitInstruction(Instruction.SetLocalMultiple);
				self.EmitByte((byte)stackIndex);
				self.EmitByte(typeSize);
			}
			else
			{
				self.EmitInstruction(Instruction.SetLocal);
				self.EmitByte((byte)stackIndex);
			}
		}

		public static void EmitLoadLocal(this CompilerIO self, int stackIndex, ValueType type)
		{
			var typeSize = type.GetSize(self.chunk);
			if (typeSize > 1)
			{
				self.EmitInstruction(Instruction.LoadLocalMultiple);
				self.EmitByte((byte)stackIndex);
				self.EmitByte(typeSize);
			}
			else
			{
				self.EmitInstruction(Instruction.LoadLocal);
				self.EmitByte((byte)stackIndex);
			}
		}
		*/
	}
}