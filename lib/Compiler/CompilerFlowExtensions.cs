namespace Maestro
{
	internal static class CompilerFlowExtensions
	{
		public static Option<Scope> GetTopCommandScope(this Compiler self)
		{
			for (var i = self.scopes.count - 1; i >= 0; i--)
			{
				var scope = self.scopes.buffer[i];
				if (scope.type == ScopeType.CommandBody)
					return scope;
			}

			return Option.None;
		}

		public static void PushScope(this Compiler self, ScopeType scopeType)
		{
			self.scopes.PushBackUnchecked(new Scope(scopeType, self.variables.count));
			self.EmitDebugInstruction(Instruction.DebugPushDebugFrame);
		}

		public static void PopScope(this Compiler self)
		{
			var scope = self.scopes.PopLast();

			for (var i = scope.variablesStartIndex; i < self.variables.count; i++)
			{
				var variable = self.variables.buffer[i];
				switch (variable.flag)
				{
				case VariableFlag.NotRead:
					self.AddSoftError(variable.slice, new CompileErrors.Variables.NotReadVariable { name = CompilerHelper.GetSlice(self, variable.slice) });
					break;
				case VariableFlag.Unwritten:
					self.AddSoftError(variable.slice, new CompileErrors.Variables.UnwrittenOutputVariable { name = CompilerHelper.GetSlice(self, variable.slice) });
					break;
				}
			}

			self.EmitDebugInstruction(Instruction.DebugPopDebugFrame);

			var localCount = self.variables.count - scope.variablesStartIndex;
			self.variables.count -= localCount;
			self.EmitPop(localCount <= byte.MaxValue ? (byte)localCount : byte.MaxValue);
		}
	}
}