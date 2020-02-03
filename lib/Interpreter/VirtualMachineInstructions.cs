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
			var tupleSizes = new Buffer<int>(8);

			void Pop(int count)
			{
				while (count-- > 0)
					stack.buffer[--stack.count] = default;
			}

			void Keep(int count)
			{
				count -= tupleSizes.PopLast();

				if (count > 0)
					stack.GrowUnchecked(count);
				else
					Pop(-count);
			}

			void CopyTail(int toIndex, int count)
			{
				var fromIndex = stack.count - count;
				while (count-- > 0)
					stack.buffer[toIndex++] = stack.buffer[fromIndex++];
			}

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

						var context = default(Context);
						context.stack = stack;
						context.inputCount = tupleSizes.PopLast();

						context.startIndex = stack.count - (context.inputCount + definition.parameterCount);

						command.Invoke(ref context);
						stack = context.stack;

						var returnCount = stack.count - (context.startIndex + context.inputCount + definition.parameterCount);
						tupleSizes.PushBackUnchecked(returnCount);

						CopyTail(context.startIndex, returnCount);
						Pop(stack.count - context.startIndex - returnCount);

						if (context.errorMessage != null)
							return vm.NewError(context.errorMessage);
						break;
					}
				case Instruction.ExecuteCommand:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);

						var instance = vm.chunk.commandInstances.buffer[index];
						var definition = vm.chunk.commandDefinitions.buffer[instance.definitionIndex];

						vm.stackFrames.buffer[vm.stackFrames.count - 1].codeIndex = codeIndex;
						codeIndex = definition.codeIndex;

						baseStackIndex = stack.count - definition.parameterCount;

						vm.stackFrames.PushBackUnchecked(new StackFrame(codeIndex, baseStackIndex, index));
						break;
					}
				case Instruction.Return:
					{
						var frame = vm.stackFrames.buffer[--vm.stackFrames.count];
						tupleSizes.SwapRemove(tupleSizes.count - 2);
						var returnCount = tupleSizes.buffer[tupleSizes.count - 1];

						CopyTail(frame.baseStackIndex, returnCount);
						Pop(stack.count - (frame.baseStackIndex + returnCount));

						frame = vm.stackFrames.buffer[vm.stackFrames.count - 1];
						codeIndex = frame.codeIndex;
						baseStackIndex = frame.baseStackIndex;
						break;
					}
				case Instruction.PushEmptyTuple:
					tupleSizes.PushBackUnchecked(0);
					break;
				case Instruction.PopTupleKeeping:
					Keep(bytes[codeIndex++]);
					break;
				case Instruction.MergeTuple:
					tupleSizes.buffer[tupleSizes.count - 2] += tupleSizes.PopLast();
					break;
				case Instruction.Pop:
					Pop(bytes[codeIndex++]);
					break;
				case Instruction.LoadFalse:
					stack.PushBackUnchecked(new Value(ValueKind.FalseKind));
					tupleSizes.PushBackUnchecked(1);
					break;
				case Instruction.LoadTrue:
					stack.PushBackUnchecked(new Value(ValueKind.TrueKind));
					tupleSizes.PushBackUnchecked(1);
					break;
				case Instruction.LoadLiteral:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						stack.PushBackUnchecked(vm.chunk.literals.buffer[index]);
						tupleSizes.PushBackUnchecked(1);
						break;
					}
				case Instruction.AssignLocal:
					{
						var index = baseStackIndex + bytes[codeIndex++];
						stack.buffer[index] = stack.PopLast();
						stack.buffer[stack.count] = default;
						break;
					}
				case Instruction.LoadLocal:
					{
						var index = baseStackIndex + bytes[codeIndex++];
						stack.PushBackUnchecked(stack.buffer[index]);
						tupleSizes.PushBackUnchecked(1);
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
				case Instruction.PopExpressionAndJumpForwardIfFalse:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);

						Keep(1);
						if (!stack.buffer[--stack.count].IsTruthy())
							codeIndex += offset;
						stack.buffer[stack.count] = default;
						break;
					}
				case Instruction.JumpForwardIfExpressionIsEmptyKeepingOne:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						if (tupleSizes.buffer[tupleSizes.count - 1] == 0)
							codeIndex += offset;
						Keep(1);
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