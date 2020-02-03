namespace Maestro
{
	public struct StackFrame
	{
		public int codeIndex;
		public int stackIndex;
		public int commandInstanceIndex;

		public StackFrame(int codeIndex, int stackIndex, int commandInstanceIndex)
		{
			this.codeIndex = codeIndex;
			this.stackIndex = stackIndex;
			this.commandInstanceIndex = commandInstanceIndex;
		}
	}

	public struct DebugInfo
	{
		public readonly struct DebugFrame
		{
			public readonly int variableInfosIndex;

			public DebugFrame(int variableInfosIndex)
			{
				this.variableInfosIndex = variableInfosIndex;
			}
		}

		public readonly struct VariableInfo
		{
			public readonly string name;
			public readonly int stackIndex;

			public VariableInfo(string name, int stackIndex)
			{
				this.name = name;
				this.stackIndex = stackIndex;
			}
		}

		public Buffer<DebugFrame> frames;
		public Buffer<VariableInfo> variableInfos;

		public void Clear()
		{
			variableInfos.ZeroClear();
		}

		public void PushFrame()
		{
			frames.PushBack(new DebugFrame(
				variableInfos.count
			));
		}

		public void PopFrame()
		{
			var frame = frames.PopLast();
			variableInfos.count = frame.variableInfosIndex;
		}
	}

	public sealed class VirtualMachine
	{
		public ByteCodeChunk chunk;
		public Buffer<StackFrame> stackFrames = new Buffer<StackFrame>(4);
		public Buffer<Value> stack = new Buffer<Value>(32);
		internal Buffer<CommandCallback> externalCommandInstances = new Buffer<CommandCallback>(8);
		public DebugInfo debugInfo;
		internal Option<IDebugger> debugger;

		internal Option<RuntimeError> Load(ByteCodeChunk chunk, ExternalCommandBindingRegistry registry)
		{
			this.chunk = chunk;

			externalCommandInstances.ZeroClear();

			for (var i = 0; i < chunk.externalCommandInstances.count; i++)
			{
				var instance = chunk.externalCommandInstances.buffer[i];
				var definition = chunk.externalCommandDefinitions.buffer[instance.definitionIndex];

				var binding = registry.Find(definition.name);
				if (!binding.isSome)
				{
					return NewError(new RuntimeErrors.ExternalCommandNotFound
					{
						name = definition.name
					});
				}

				if (!binding.value.definition.IsEqualTo(definition))
				{
					return NewError(new RuntimeErrors.IncompatibleExternalCommand
					{
						name = definition.name,
						expectedParameters = definition.parameterCount,
						gotParameters = binding.value.definition.parameterCount,
					});
				}

				var command = binding.value.factory();
				externalCommandInstances.PushBackUnchecked(command);
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