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

						vm.stack = stack;
						instance.Invoke(vm, inputCount);
						stack = vm.stack;

						for (var i = stack.count; i < previousStackCount; i++)
							stack.buffer[i] = default;
						break;
					}
				case Instruction.Pop:
					stack.buffer[--stack.count] = default;
					break;
				case Instruction.PopMultiple:
					{
						var count = bytes[codeIndex++];
						while (--count >= 0)
							stack.buffer[--stack.count] = default;
					}
					break;
				case Instruction.LoadNull:
					stack.PushBackUnchecked(new Value(null));
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
				case Instruction.AddLocalName:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						vm.localVariableNames.PushBackUnchecked(vm.chunk.literals.buffer[index].asObject as string);
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
				case Instruction.PopLocals:
					{
						var count = bytes[codeIndex++];
						vm.localVariableNames.count -= count;
						while (--count >= 0)
							stack.buffer[--stack.count] = default;
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
				default:
					goto case Instruction.Halt;
				}
			}
		}
	}
}