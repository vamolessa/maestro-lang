namespace Flow
{
	internal static class CompilerFlowExtensions
	{
		public static Scope BeginScope(this Compiler self)
		{
			self.scopeDepth += 1;
			return new Scope(self.localVariables.count);
		}

		public static void EndScope(this Compiler self, Scope scope)
		{
			self.scopeDepth -= 1;

			for (var i = scope.localVariablesStartIndex; i < self.localVariables.count; i++)
			{
				var local = self.localVariables.buffer[i];
				if (!local.used)
				{
					self.AddSoftError(local.slice, new LocalVariableNotUsedError { name = CompilerHelper.GetSlice(self, local.slice) });
				}
			}

			var localCount = self.localVariables.count - scope.localVariablesStartIndex;

			if (localCount > 0)
			{
				self.EmitInstruction(Instruction.PopLocals);
				self.EmitByte((byte)localCount);
			}

			self.localVariables.count -= localCount;
		}
	}
}