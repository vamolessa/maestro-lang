#define DEBUG_TRACE
using System.Text;

namespace Flow
{
	internal static class VirtualMachineInstructions
	{
		public static Option<RuntimeError> Execute(VirtualMachine vm)
		{
#if DEBUG_TRACE
			var debugSb = new StringBuilder();
#endif

			var bytes = vm.chunk.bytes.buffer;
			var stack = vm.stack;
			var codeIndex = vm.stackFrames.buffer[vm.stackFrames.count - 1].codeIndex;
			var baseStackIndex = vm.stackFrames.buffer[vm.stackFrames.count - 1].baseStackIndex;

			while (true)
			{
#if DEBUG_TRACE
				switch ((Instruction)bytes[codeIndex])
				{
				case Instruction.DebugHook:
				case Instruction.DebugPushLocalInfo:
				case Instruction.DebugPopLocalInfos:
					break;
				default:
					debugSb.Clear();
					vm.stack = stack;
					vm.stackFrames.buffer[vm.stackFrames.count - 1].baseStackIndex = baseStackIndex;
					VirtualMachineHelper.TraceStack(vm, debugSb);
					vm.chunk.DisassembleInstruction(codeIndex, debugSb);
					System.Console.WriteLine(debugSb);
					break;
				}
#endif

				var nextInstruction = (Instruction)bytes[codeIndex++];
				switch (nextInstruction)
				{
				case Instruction.Halt:
					--vm.stackFrames.count;
					vm.stack = stack;
					return Option.None;
				case Instruction.ExecuteNativeCommand:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);

						var instance = vm.chunk.externalCommandInstances.buffer[index];
						var definition = vm.chunk.externalCommandDefinitions.buffer[instance.definitionIndex];
						var command = vm.externalCommandInstances.buffer[index];

						var previousStackCount = stack.count;
						stack.count -= definition.parameterCount;
						var inputCount = stack.buffer[--stack.count].asNumber.asInt;
						stack.count -= inputCount;

						var inputs = new Inputs(inputCount, stack.count, stack.buffer);
						stack.GrowUnchecked(definition.returnCount);

						var error = command.Invoke(inputs);

						while (previousStackCount > stack.count)
							stack.buffer[--previousStackCount] = default;

						if (error is IFormattedMessage errorMessage)
							return vm.NewError(errorMessage);
						break;
					}
				case Instruction.ExecuteCommand:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);

						var instance = vm.chunk.commandInstances.buffer[index];
						var definition = vm.chunk.commandDefinitions.buffer[instance.definitionIndex];

						vm.stackFrames.buffer[vm.stackFrames.count - 1].codeIndex = codeIndex;
						codeIndex = definition.codeIndex;

						baseStackIndex = stack.count - 1 - definition.parameterCount;
						stack.GrowUnchecked(definition.returnCount);

						vm.stackFrames.PushBackUnchecked(new StackFrame(codeIndex, baseStackIndex, index));
						break;
					}
				case Instruction.Return:
					{
						var frame = vm.stackFrames.buffer[--vm.stackFrames.count];
						var instance = vm.chunk.commandInstances.buffer[frame.commandInstanceIndex];
						var definition = vm.chunk.commandDefinitions.buffer[instance.definitionIndex];

						var inputCount = stack.buffer[frame.baseStackIndex].asNumber.asInt;
						stack.count = frame.baseStackIndex - inputCount;
						var returnStartIndex = frame.baseStackIndex + 1 + definition.parameterCount;

						for (var i = 0; i < definition.returnCount; i++)
							stack.buffer[stack.count++] = stack.buffer[returnStartIndex++];

						while (returnStartIndex > stack.count)
							stack.buffer[--returnStartIndex] = default;

						frame = vm.stackFrames.buffer[vm.stackFrames.count - 1];
						codeIndex = frame.codeIndex;
						baseStackIndex = frame.baseStackIndex;
						break;
					}
				case Instruction.Pop:
					stack.buffer[--stack.count] = default;
					break;
				case Instruction.PopMultiple:
					{
						var count = bytes[codeIndex++];
						while (count-- > 0)
							stack.buffer[--stack.count] = default;
					}
					break;
				case Instruction.LoadFalse:
					stack.PushBackUnchecked(new Value(ValueKind.FalseKind));
					break;
				case Instruction.LoadTrue:
					stack.PushBackUnchecked(new Value(ValueKind.TrueKind));
					break;
				case Instruction.LoadLiteral:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						stack.PushBackUnchecked(vm.chunk.literals.buffer[index]);
						break;
					}
				case Instruction.AssignLocal:
					{
						var index = baseStackIndex + bytes[codeIndex++];
						stack.buffer[index] = stack.buffer[--stack.count];
						stack.buffer[stack.count] = default;
						break;
					}
				case Instruction.LoadLocal:
					{
						var index = baseStackIndex + bytes[codeIndex++];
						stack.PushBackUnchecked(stack.buffer[index]);
						break;
					}
				case Instruction.JumpBackward:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						codeIndex -= offset;
						break;
					}
				case Instruction.JumpForward:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						codeIndex += offset;
						break;
					}
				case Instruction.PopAndJumpForwardIfFalse:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						if (!stack.buffer[--stack.count].IsTruthy())
							codeIndex += offset;
						stack.buffer[stack.count] = default;
						break;
					}
				case Instruction.JumpForwardIfNull:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						if (stack.buffer[stack.count - 1].asObject is null)
							codeIndex += offset;
						break;
					}
				case Instruction.DebugHook:
					if (vm.debugger.isSome)
					{
						vm.stack = stack;
						vm.stackFrames.buffer[vm.stackFrames.count - 1].codeIndex = codeIndex;
						vm.debugger.value.OnDebugHook();
					}
					break;
				case Instruction.DebugPushLocalInfo:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						var name = vm.chunk.literals.buffer[index].asObject as string;
						vm.debugInfo.localVariables.PushBack(new DebugInfo.VariableInfo(name));
						break;
					}
				case Instruction.DebugPopLocalInfos:
					vm.debugInfo.localVariables.count -= bytes[codeIndex++];
					break;
				default:
					goto case Instruction.Halt;
				}
			}
		}
	}
}