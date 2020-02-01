namespace Flow
{
	internal static class CompilerFlowExtensions
	{
		public static Scope BeginScope(this Compiler self)
		{
			return new Scope(self.localVariables.count);
		}

		public static int EndScopeKeepingLocalValues(this Compiler self, Scope scope)
		{
			for (var i = scope.localVariablesStartIndex; i < self.localVariables.count; i++)
			{
				var local = self.localVariables.buffer[i];
				switch (local.flag)
				{
				case LocalVariableFlag.NotRead:
					self.AddSoftError(local.slice, new CompileErrors.Variables.NotReadLocalVariable { name = CompilerHelper.GetSlice(self, local.slice) });
					break;
				case LocalVariableFlag.Unwritten:
					self.AddSoftError(local.slice, new CompileErrors.Variables.UnwrittenOutputVariable { name = CompilerHelper.GetSlice(self, local.slice) });
					break;
				}
			}

			var localCount = self.localVariables.count - scope.localVariablesStartIndex;

			self.localVariables.count -= localCount;
			self.EmitPopLocalsInfo(localCount);

			return localCount;
		}

		public static int EndScope(this Compiler self, Scope scope)
		{
			var localCount = self.EndScopeKeepingLocalValues(scope);
			self.EmitInstruction(Instruction.PopMultipleExpressions);
			self.EmitByte(localCount <= byte.MaxValue ? (byte)localCount : byte.MaxValue);
			self.EmitInstruction(Instruction.PopOneExpression);
			return localCount;
		}
	}
}