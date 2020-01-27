namespace Flow
{
	internal static class CompilerDeclarationExtensions
	{
		public static byte AddLocalVariable(this Compiler self, Slice slice, LocalVariableFlag flag)
		{
			var name = flag != LocalVariableFlag.Input ?
				CompilerHelper.GetSlice(self, slice) :
				"$$";
			var nameLiteralIndex = self.chunk.AddLiteral(new Value(name));

			self.EmitInstruction(Instruction.PushLocalInfo);
			self.EmitUShort((ushort)nameLiteralIndex);

			if (
				flag == LocalVariableFlag.Unused &&
				self.parser.tokenizer.source[slice.index + 1] == '_'
			)
				flag = LocalVariableFlag.Used;

			self.localVariables.PushBack(new LocalVariable(slice, flag));

			return unchecked((byte)(self.localVariables.count - 1));
		}

		public static bool ResolveToLocalVariableIndex(this Compiler self, Slice slice, out byte index)
		{
			var source = self.parser.tokenizer.source;

			if (CompilerHelper.AreEqual(source, slice, "$$"))
			{
				for (var i = self.localVariables.count - 1; i >= 0; i--)
				{
					var local = self.localVariables.buffer[i];
					if (local.flag == LocalVariableFlag.Input)
					{
						index = unchecked((byte)i);
						return true;
					}
				}
			}

			for (var i = self.localVariables.count - 1; i >= 0; i--)
			{
				var local = self.localVariables.buffer[i];
				if (CompilerHelper.AreEqual(source, slice, local.slice))
				{
					index = unchecked((byte)i);
					return true;
				}
			}

			index = 0;
			return false;
		}
	}
}