namespace Flow
{
	internal static class CompilerDeclarationExtensions
	{
		public static byte AddLocalVariable(this Compiler self, Slice slice, LocalVariableFlag flag)
		{
			self.EmitPushLocalInfo(slice, flag);

			if (
				flag == LocalVariableFlag.NotRead &&
				self.parser.tokenizer.source[slice.index + 1] == '_'
			)
			{
				flag = LocalVariableFlag.Fulfilled;
			}

			self.localVariables.PushBack(new LocalVariable(slice, flag));

			return unchecked((byte)(self.localVariables.count - 1));
		}

		public static Option<byte> ResolveToLocalVariableIndex(this Compiler self, Slice slice)
		{
			Option<byte> CheckAndReturnIndex(int localIndex)
			{
				if (localIndex < self.baseVariableIndex)
				{
					self.AddSoftError(slice, new CompileErrors.Variables.CanNotAccessVariableOutsideOfScope
					{
						name = CompilerHelper.GetSlice(self, slice)
					});

					return Option.None;
				}
				else
				{
					return localIndex < byte.MaxValue ?
						(byte)localIndex :
						byte.MaxValue;
				}
			}

			var source = self.parser.tokenizer.source;

			if (CompilerHelper.AreEqual(source, slice, "$$"))
			{
				for (var i = self.localVariables.count - 1; i >= 0; i--)
				{
					var local = self.localVariables.buffer[i];
					if (local.flag == LocalVariableFlag.Input)
						return CheckAndReturnIndex(i);
				}
			}

			for (var i = self.localVariables.count - 1; i >= 0; i--)
			{
				var local = self.localVariables.buffer[i];
				if (CompilerHelper.AreEqual(source, slice, local.slice))
					return CheckAndReturnIndex(i);
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