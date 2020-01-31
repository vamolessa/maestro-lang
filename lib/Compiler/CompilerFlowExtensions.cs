namespace Flow
{
	internal static class CompilerFlowExtensions
	{
		public static void CheckVariablesFulfillment(this Compiler self, Scope scope)
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
		}

		public static Scope BeginScope(this Compiler self)
		{
			return new Scope(self.localVariables.count);
		}

		public static void EndScope(this Compiler self, Scope scope)
		{
			self.CheckVariablesFulfillment(scope);

			var localCount = self.localVariables.count - scope.localVariablesStartIndex;
			self.localVariables.count -= localCount;

			self.EmitPopLocalsInfo(localCount);
			self.EmitPop(localCount);

		}
	}
}