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
				if (local.flag == LocalVariableFlag.Unused)
				{
					self.AddSoftError(local.slice, new CompileErrors.Variables.UnusedLocalVariable { name = CompilerHelper.GetSlice(self, local.slice) });
				}
			}

			var localCount = self.localVariables.count - scope.localVariablesStartIndex;

			if (localCount > 0)
			{
				self.EmitInstruction(Instruction.PopLocalInfos);
				self.EmitByte((byte)localCount);
				self.EmitPop(localCount);
			}

			self.localVariables.count -= localCount;
		}
	}
}