namespace Flow
{
	public struct StackFrame
	{
		public int codeIndex;
		public int stackIndex;

		public StackFrame(int codeIndex, int stackIndex)
		{
			this.codeIndex = codeIndex;
			this.stackIndex = stackIndex;
		}
	}

	public readonly struct VariableInfo
	{
		public readonly string name;

		public VariableInfo(string name)
		{
			this.name = name;
		}
	}

	internal sealed class VirtualMachine
	{
		internal ByteCodeChunk chunk;
		internal Buffer<StackFrame> stackFrames = new Buffer<StackFrame>(4);
		internal Buffer<Value> stack = new Buffer<Value>(32);
		internal Buffer<ICommand> commandInstances = new Buffer<ICommand>(8);
		internal Buffer<VariableInfo> localVariableInfos = new Buffer<VariableInfo>(8);

		internal Option<RuntimeError> Load(ByteCodeChunk chunk, ExternalCommandBindingRegistry registry)
		{
			this.chunk = chunk;

			stackFrames.count = 0;
			stack.ZeroReset();
			commandInstances.ZeroReset();
			localVariableInfos.ZeroReset();

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