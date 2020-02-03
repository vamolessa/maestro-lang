#define DEBUG_TRACE
using System.Text;

namespace Maestro
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
			var frameStackIndex = vm.stackFrames.buffer[vm.stackFrames.count - 1].stackIndex;

			var tupleSizes = new Buffer<int>(2);
			var inputSlices = new Buffer<Slice>(2);

			void Keep(int count)
			{
				count -= tupleSizes.PopLast();

				if (count > 0)
					stack.GrowUnchecked(count);
				else
					stack.count += count;
			}

			void MoveTail(int toIndex, int count)
			{
				var fromIndex = stack.count - count;
				while (count-- > 0)
					stack.buffer[toIndex++] = stack.buffer[fromIndex++];
				stack.count = toIndex;
			}

			while (true)
			{
#if DEBUG_TRACE
				switch ((Instruction)bytes[codeIndex])
				{
				case Instruction.DebugHook:
				case Instruction.DebugPushDebugFrame:
				case Instruction.DebugPopDebugFrame:
				case Instruction.DebugPushVariableInfo:
					break;
				default:
					debugSb.Clear();
					vm.stack = stack;
					vm.stackFrames.buffer[vm.stackFrames.count - 1].stackIndex = frameStackIndex;
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

						MoveTail(context.startIndex, returnCount);

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

						frameStackIndex = stack.count - definition.parameterCount;

						var inputCount = tupleSizes.PopLast();
						inputSlices.PushBackUnchecked(new Slice(frameStackIndex - inputCount, inputCount));

						vm.stackFrames.PushBackUnchecked(new StackFrame(codeIndex, frameStackIndex, index));
						break;
					}
				case Instruction.Return:
					{
						var frame = vm.stackFrames.buffer[--vm.stackFrames.count];

						var inputSlice = inputSlices.PopLast();
						MoveTail(inputSlice.index, tupleSizes.buffer[tupleSizes.count - 1]);
						;

						frame = vm.stackFrames.buffer[vm.stackFrames.count - 1];
						codeIndex = frame.codeIndex;
						frameStackIndex = frame.stackIndex;
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
					stack.count -= bytes[codeIndex++];
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
						var index = frameStackIndex + bytes[codeIndex++];
						stack.buffer[index] = stack.PopLast();
						break;
					}
				case Instruction.LoadLocal:
					{
						var index = frameStackIndex + bytes[codeIndex++];
						stack.PushBackUnchecked(stack.buffer[index]);
						tupleSizes.PushBackUnchecked(1);
						break;
					}
				case Instruction.LoadInput:
					{
						var slice = inputSlices.buffer[inputSlices.count - 1];
						for (var i = 0; i < slice.length; i++)
							stack.PushBackUnchecked(stack.buffer[slice.index + i]);
						tupleSizes.PushBackUnchecked(slice.length);
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
				case Instruction.IfConditionJump:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);

						Keep(1);
						if (!stack.buffer[--stack.count].IsTruthy())
							codeIndex += offset;
						break;
					}
				case Instruction.IterateConditionJump:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);

						var inputCount = tupleSizes.PopLast();
						if (inputCount == 0)
						{
							--inputSlices.count;
							codeIndex += offset;
						}
						else
						{
							inputSlices.PushBackUnchecked(new Slice(stack.count - inputCount, inputCount));
						}
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
				case Instruction.DebugPushDebugFrame:
					vm.debugInfo.PushFrame();
					break;
				case Instruction.DebugPopDebugFrame:
					vm.debugInfo.PopFrame();
					break;
				case Instruction.DebugPushVariableInfo:
					{
						var nameIndex = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						var name = vm.chunk.literals.buffer[nameIndex].asObject as string;
						var stackIndex = frameStackIndex + bytes[codeIndex++];
						vm.debugInfo.variableInfos.PushBack(new DebugInfo.VariableInfo(name, stackIndex));
						break;
					}
				default:
					goto case Instruction.Halt;
				}
			}
		}
	}
}