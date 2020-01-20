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
			var codeIndex = vm.callFrameStack.buffer[vm.callFrameStack.count - 1].codeIndex;
			var baseStackIndex = vm.callFrameStack.buffer[vm.callFrameStack.count - 1].baseStackIndex;

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
					--vm.callFrameStack.count;
					vm.stack = stack;
					return;
				case Instruction.CallNativeCommand:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						var commandIndex = vm.chunk.commandInstances.buffer[index];
						var command = vm.chunk.commands.buffer[commandIndex];
						var instance = vm.commands.buffer[index];

						stack.count -= command.parameterCount;
						var result = instance.Invoke(stack.buffer[stack.count - 1]);
						stack.buffer[stack.count - 1] = result;
						break;
					}
				case Instruction.ClearStack:
					stack.count = 0;
					vm.localVariableNames.count = 0;
					break;
				case Instruction.Pop:
					--stack.count;
					break;
				case Instruction.LoadNull:
					stack.PushBackUnchecked(null);
					break;
				case Instruction.LoadFalse:
					stack.PushBackUnchecked(false);
					break;
				case Instruction.LoadTrue:
					stack.PushBackUnchecked(true);
					break;
				case Instruction.LoadLiteral:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						stack.PushBackUnchecked(vm.chunk.literals.buffer[index]);
						break;
					}
				case Instruction.CreateArray:
					{
						var array = new object[bytes[codeIndex++]];
						stack.count -= array.Length;
						for (var i = 0; i < array.Length; i++)
							array[i] = stack.buffer[stack.count + i];
						stack.PushBackUnchecked(array);
						break;
					}
				case Instruction.AddLocalName:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						vm.localVariableNames.PushBackUnchecked(vm.chunk.literals.buffer[index] as string);
						break;
					}
				case Instruction.AssignLocal:
					{
						var index = baseStackIndex + bytes[codeIndex++];
						stack.buffer[index] = stack.buffer[stack.count - 1];
						break;
					}
				case Instruction.LoadLocal:
					{
						var index = baseStackIndex + bytes[codeIndex++];
						stack.PushBackUnchecked(stack.buffer[index]);
						break;
					}
				default:
					goto case Instruction.Halt;
				}
			}
		}
	}
}