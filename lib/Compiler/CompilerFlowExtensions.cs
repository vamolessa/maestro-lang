namespace Flow
{
	internal static class CompilerFlowExtensions
	{
		public static Scope BeginScope(this Compiler self)
		{
			return new Scope(self.localVariables.count);
		}

		public static void EndScope(this Compiler self, Scope scope)
		{
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
				if (self.mode == Mode.Debug)
				{
					self.EmitInstruction(Instruction.DebugPopLocalInfos);
					self.EmitByte((byte)localCount);
				}

				self.EmitPop(localCount);
			}

			self.localVariables.count -= localCount;
		}
	}
}