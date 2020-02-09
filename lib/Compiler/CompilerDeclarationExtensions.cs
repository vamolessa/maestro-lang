namespace Maestro
{
	internal static class CompilerDeclarationExtensions
	{
		public static int AddVariable(this Compiler self, Slice slice, VariableFlag flag)
		{
			self.EmitDebugPushVariableInfo(slice);

			if (
				flag == VariableFlag.NotRead &&
				self.parser.tokenizer.source[slice.index + 1] == '_'
			)
			{
				flag = VariableFlag.Fulfilled;
			}

			self.variables.PushBack(new Variable(slice, flag));
			return self.variables.count - 1;
		}

		public static Option<int> ResolveToVariableIndex(this Compiler self, Slice slice)
		{
			var source = self.parser.tokenizer.source;

			for (var i = self.variables.count - 1; i >= 0; i--)
			{
				var variable = self.variables.buffer[i];
				if (!CompilerHelper.AreEqual(source, slice, variable.slice))
					continue;

				var commandVariablesBaseIndex = self.GetTopCommandScope().Select(s => s.variablesStartIndex).GetOr(0);
				if (i < commandVariablesBaseIndex)
				{
					self.AddSoftError(slice, new CompileErrors.Variables.CanNotAccessVariableOutsideOfCommandScope
					{
						name = CompilerHelper.GetSlice(self, slice)
					});

					return Option.None;
				}

				return i;
			}

			return Option.None;
		}

		public static Option<int> ResolveToExternalCommandIndex(this Compiler self, Slice slice)
		{
			for (var i = 0; i < self.chunk.externalCommandDefinitions.count; i++)
			{
				var command = self.chunk.externalCommandDefinitions.buffer[i];
				if (CompilerHelper.AreEqual(self.parser.tokenizer.source, slice, command.name))
					return i;
			}

			return Option.None;
		}

		public static Option<int> ResolveToCommandIndex(this Compiler self, Slice slice)
		{
			for (var i = 0; i < self.chunk.commandDefinitions.count; i++)
			{
				var command = self.chunk.commandDefinitions.buffer[i];
				if (CompilerHelper.AreEqual(self.parser.tokenizer.source, slice, command.name))
					return i;
			}

			return Option.None;
		}
	}
}