namespace Maestro
{
	internal readonly struct ExternalCommandReference
	{
		public readonly Assembly assembly;
		public readonly byte dependencyIndex;
		public readonly byte commandIndex;

		public ExternalCommandReference(Assembly assembly, byte dependencyIndex, byte commandIndex)
		{
			this.assembly = assembly;
			this.dependencyIndex = dependencyIndex;
			this.commandIndex = commandIndex;
		}
	}

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

		public static Option<int> ResolveToNativeCommandIndex(this Compiler self, NativeCommandBindingRegistry bindingRegistry, Slice slice)
		{
			for (var i = 0; i < self.assembly.nativeCommandDefinitions.count; i++)
			{
				var command = self.assembly.nativeCommandDefinitions.buffer[i];
				if (CompilerHelper.AreEqual(self.parser.tokenizer.source, slice, command.name))
					return i;
			}

			for (var i = 0; i < bindingRegistry.bindings.count; i++)
			{
				var definition = bindingRegistry.bindings.buffer[i].definition;
				if (CompilerHelper.AreEqual(self.parser.tokenizer.source, slice, definition.name))
				{
					var index = self.assembly.nativeCommandDefinitions.count;
					if (self.assembly.AddNativeCommand(definition))
						return index;
					else
						break;
				}
			}

			return Option.None;
		}

		public static Option<byte> ResolveToCommandIndex(this Compiler self, Slice slice)
		{
			for (var i = 0; i < self.assembly.commandDefinitions.count; i++)
			{
				var command = self.assembly.commandDefinitions.buffer[i];
				if (CompilerHelper.AreEqual(self.parser.tokenizer.source, slice, command.name))
				{
					return i < byte.MaxValue ? (byte)i : byte.MaxValue;
				}
			}

			return Option.None;
		}
	}
}