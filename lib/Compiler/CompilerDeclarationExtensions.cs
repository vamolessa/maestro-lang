namespace Flow
{
	internal static class CompilerDeclarationExtensions
	{
		public static int AddLocalVariable(this Compiler self, Slice slice, byte size, bool used)
		{
			var name = CompilerHelper.GetSlice(self, slice);
			var nameLiteralIndex = self.chunk.AddLiteral(new Value(name));
			self.EmitInstruction(Instruction.PushLocalInfo);
			self.EmitUShort((ushort)nameLiteralIndex);
			self.EmitByte(size);

			used = used || self.parser.tokenizer.source[slice.index + 1] == '_';
			self.localVariables.PushBack(new LocalVariable(slice, size, used));
			return self.localVariables.count - 1;
		}

		public static bool ResolveToLocalVariableIndex(this Compiler self, Slice slice, out int index)
		{
			var source = self.parser.tokenizer.source;

			for (var i = self.localVariables.count - 1; i >= 0; i--)
			{
				var local = self.localVariables.buffer[i];
				if (CompilerHelper.AreEqual(source, slice, local.slice))
				{
					index = i;
					return true;
				}
			}

			index = 0;
			return false;
		}

		public static byte GetLocalVariableStackIndex(this Compiler self, int localVarIndex)
		{
			var stackIndex = 0;
			for (var i = 0; i < localVarIndex; i++)
			{
				var localVar = self.localVariables.buffer[i];
				stackIndex += localVar.size;
			}

			if (stackIndex > byte.MaxValue)
			{
				self.AddSoftError(self.parser.previousToken.slice, new VariableTooDeepToBeAddressedError());
				return 0;
			}

			return (byte)stackIndex;
		}
	}
}