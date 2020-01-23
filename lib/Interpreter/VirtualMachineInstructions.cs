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
						var command = vm.chunk.commandDefinitions.buffer[commandIndex];
						var instance = vm.commands.buffer[index];

						var args = vm.CreateArray(command.parameterCount);
						stack.count -= args.Length;
						for (var i = 0; i < args.Length; i++)
							args[i] = stack.buffer[stack.count + i];

						var result = instance.Invoke(stack.buffer[stack.count - 1], args);
						result = VirtualMachineHelper.DeepCopy(vm, result);

						for (var i = 0; i < args.Length; i++)
							VirtualMachineHelper.Collect(vm, ref stack.buffer[stack.count + i]);

						stack.buffer[stack.count - 1] = result;
						break;
					}
				case Instruction.Pop:
					VirtualMachineHelper.Collect(vm, ref stack.buffer[--stack.count]);
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
				case Instruction.CreateArray:
					{
						var array = vm.arrayPool.Request(bytes[codeIndex++]);
						stack.count -= array.Length;
						for (var i = 0; i < array.Length; i++)
							array[i] = stack.buffer[stack.count + i];
						stack.PushBackUnchecked(new Value(array));
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
						stack.buffer[index] = stack.buffer[stack.count - 1];
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
						stack.count -= count;

						for (var i = 0; i < count; i++)
							VirtualMachineHelper.Collect(vm, ref stack.buffer[stack.count + i]);
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
						ref var value = ref stack.buffer[--stack.count];
						if (!value.IsTruthy())
							codeIndex += offset;
						VirtualMachineHelper.Collect(vm, ref value);
						break;
					}
				default:
					goto case Instruction.Halt;
				}
			}
		}
	}
}