namespace Flow
{
	public struct StackFrame
	{
		public int codeIndex;
		public int stackIndex;
		public int commandDefinitionIndex;

		public StackFrame(int codeIndex, int stackIndex, int commandDefinitionIndex)
		{
			this.codeIndex = codeIndex;
			this.stackIndex = stackIndex;
			this.commandDefinitionIndex = commandDefinitionIndex;
		}
	}

	public struct DebugInfo
	{
		public readonly struct VariableInfo
		{
			public readonly string name;

			public VariableInfo(string name)
			{
				this.name = name;
			}
		}

		public Buffer<VariableInfo> localVariables;

		public void Clear()
		{
			localVariables.ZeroReset();
		}
	}

	public sealed class VirtualMachine
	{
		public ByteCodeChunk chunk;
		public Buffer<StackFrame> stackFrames = new Buffer<StackFrame>(4);
		public Buffer<Value> stack = new Buffer<Value>(32);
		internal Buffer<CommandCallback> commandInstances = new Buffer<CommandCallback>(8);
		public DebugInfo debugInfo;
		internal Option<IDebugger> debugger;

		internal Option<RuntimeError> Load(ByteCodeChunk chunk, ExternalCommandBindingRegistry registry)
		{
			this.chunk = chunk;

			stackFrames.count = 0;
			stack.ZeroReset();
			commandInstances.ZeroReset();
			debugInfo.Clear();

			for (var i = 0; i < chunk.commandInstances.count; i++)
			{
				var index = chunk.commandInstances.buffer[i];
				var command = chunk.commandDefinitions.buffer[index];

				var binding = registry.Find(command.name);
				if (!binding.isSome)
				{
					return NewError(new RuntimeErrors.ExternalCommandNotFound
					{
						name = command.name
					});
				}

				if (!binding.value.definition.IsEqualTo(command))
				{
					return NewError(new RuntimeErrors.IncompatibleExternalCommand
					{
						name = command.name,
						expectedParameters = command.parameterCount,
						expectedReturns = command.returnCount,
						gotParameters = binding.value.definition.parameterCount,
						gotReturns = binding.value.definition.returnCount
					});
				}

				var instance = binding.value.factory();
				commandInstances.PushBackUnchecked(instance);
			}

			return Option.None;
		}

		public RuntimeError NewError(IFormattedMessage message)
		{
			var ip = -1;
			if (stackFrames.count > 0)
				ip = stackFrames.buffer[stackFrames.count - 1].codeIndex;

			return new RuntimeError(
				ip,
				ip >= 0 ? chunk.sourceSlices.buffer[ip] : new Slice(),
				message
			);
		}
	}
}