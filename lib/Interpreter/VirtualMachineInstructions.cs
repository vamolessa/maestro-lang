#define DEBUG_TRACE
using System.Text;

namespace Flow
{
	internal static class VirtualMachineInstructions
	{
		public static void Run(VirtualMachine vm)
		{
#if DEBUG_TRACE
			var debugSb = new StringBuilder();
#endif

			var bytes = vm.chunk.bytes.buffer;
			var stack = vm.stack;
			var codeIndex = vm.stackFrames.buffer[vm.stackFrames.count - 1].codeIndex;
			var baseStackIndex = vm.stackFrames.buffer[vm.stackFrames.count - 1].stackIndex;

			while (true)
			{
#if DEBUG_TRACE
				debugSb.Clear();
				vm.stack = stack;
				VirtualMachineHelper.TraceStack(vm, debugSb);
				vm.chunk.DisassembleInstruction(codeIndex, debugSb);
				System.Console.WriteLine(debugSb);
#endif

				var nextInstruction = (Instruction)bytes[codeIndex++];
				switch (nextInstruction)
				{
				case Instruction.Halt:
					--vm.stackFrames.count;
					vm.stack = stack;
					return;
				case Instruction.CallNativeCommand:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						var inputCount = bytes[codeIndex++];

						var commandIndex = vm.chunk.commandInstances.buffer[index];
						var command = vm.chunk.commandDefinitions.buffer[commandIndex];
						var instance = vm.commands.buffer[index];

						var previousStackCount = stack.count;
						stack.count -= inputCount + command.parameterCount;
						var inputs = new Inputs(inputCount, stack.count, stack.buffer);
						stack.GrowUnchecked(command.returnCount);

						instance.Invoke(inputs);

						while (previousStackCount > stack.count)
							stack.buffer[previousStackCount--] = default;
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
				case Instruction.PushLocalInfo:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						var name = vm.chunk.literals.buffer[index].asObject as string;
						vm.localVariableInfos.PushBackUnchecked(new VariableInfo(name));
						break;
					}
				case Instruction.PopLocalInfos:
					vm.localVariableInfos.count -= bytes[codeIndex++];
					break;
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
				default:
					goto case Instruction.Halt;
				}
			}
		}
	}
}