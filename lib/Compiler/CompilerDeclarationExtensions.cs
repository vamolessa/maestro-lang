namespace Flow
{
	internal static class CompilerDeclarationExtensions
	{
		public static int AddLocalVariable(this Compiler self, Slice slice, bool used)
		{
			var name = CompilerHelper.GetSlice(self, slice);
			var nameLiteralIndex = self.chunk.AddLiteral(name);
			self.EmitInstruction(Instruction.AddLocalName);
			self.EmitUShort((ushort)nameLiteralIndex);

			used = used || self.parser.tokenizer.source[slice.index + 1] == '_';
			self.localVariables.PushBack(new LocalVariable(slice, used));
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
	}
}