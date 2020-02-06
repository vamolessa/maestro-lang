#define DEBUG_TRACE
using System.Text;

namespace Maestro
{
	internal static class VirtualMachineInstructions
	{
		public static Option<RuntimeError> Execute(this VirtualMachine vm, ByteCodeChunk chunk, ExternalCommandCallback[] externalCommandInstances)
		{
#if DEBUG_TRACE
			var debugSb = new StringBuilder();
#endif

			var bytes = chunk.bytes.buffer;
			var stack = vm.stack;
			var codeIndex = vm.stackFrames.buffer[vm.stackFrames.count - 1].codeIndex;
			var frameStackIndex = vm.stackFrames.buffer[vm.stackFrames.count - 1].stackIndex;

			var tupleSizes = vm.tupleSizes;
			var inputSlices = vm.inputSlices;

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
					vm.TraceStack(debugSb);
					chunk.DisassembleInstruction(codeIndex, debugSb);
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
					vm.tupleSizes = tupleSizes;
					vm.inputSlices = inputSlices;
					return Option.None;
				case Instruction.ExecuteNativeCommand:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);

						var definitionIndex = chunk.externalCommandInstances.buffer[index].definitionIndex;
						var parameterCount = chunk.externalCommandDefinitions.buffer[definitionIndex].parameterCount;

						var context = default(Context);
						context.stack = stack;
						context.inputCount = tupleSizes.PopLast();

						context.startIndex = stack.count - (context.inputCount + parameterCount);

						externalCommandInstances[index].Invoke(ref context);
						stack = context.stack;

						var returnCount = stack.count - (context.startIndex + context.inputCount + parameterCount);
						tupleSizes.PushBackUnchecked(returnCount);

						MoveTail(context.startIndex, returnCount);

						if (context.errorMessage != null)
							return vm.NewError(chunk, context.errorMessage);
						break;
					}
				case Instruction.ExecuteCommand:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						var definition = chunk.commandDefinitions.buffer[index];

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
					{
						var count = bytes[codeIndex++] - tupleSizes.PopLast();
						if (count > 0)
							stack.GrowUnchecked(count);
						else
							stack.count += count;
						break;
					}
				case Instruction.MergeTuple:
					tupleSizes.buffer[tupleSizes.count - 2] += tupleSizes.PopLast();
					break;
				case Instruction.Pop:
					stack.count -= bytes[codeIndex++];
					break;
				case Instruction.PushFalse:
					stack.PushBackUnchecked(new Value(ValueKind.FalseKind));
					tupleSizes.PushBackUnchecked(1);
					break;
				case Instruction.PushTrue:
					stack.PushBackUnchecked(new Value(ValueKind.TrueKind));
					tupleSizes.PushBackUnchecked(1);
					break;
				case Instruction.PushLiteral:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						stack.PushBackUnchecked(chunk.literals.buffer[index]);
						tupleSizes.PushBackUnchecked(1);
						break;
					}
				case Instruction.SetLocal:
					{
						var index = frameStackIndex + bytes[codeIndex++];
						stack.buffer[index] = stack.PopLast();
						break;
					}
				case Instruction.PushLocal:
					{
						var index = frameStackIndex + bytes[codeIndex++];
						stack.PushBackUnchecked(stack.buffer[index]);
						tupleSizes.PushBackUnchecked(1);
						break;
					}
				case Instruction.PushInput:
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

						var count = tupleSizes.PopLast();
						while (count-- > 0)
						{
							if (!stack.buffer[--stack.count].IsTruthy())
							{
								codeIndex += offset;
								while (count-- > 0)
									--stack.count;
								break;
							}
						}
						break;
					}
				case Instruction.ForEachConditionJump:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);

						var elementCount = tupleSizes.buffer[tupleSizes.count - 1];
						var baseIndex = stack.count - (elementCount + 2);
						var currentIndex = stack.buffer[baseIndex].asNumber.asInt + 1;

						if (currentIndex < elementCount)
						{
							stack.buffer[baseIndex + 1] = stack.buffer[baseIndex + 2 + currentIndex];
							stack.buffer[baseIndex] = new Value(currentIndex);
						}
						else
						{
							codeIndex += offset;
							stack.count -= elementCount;
							--tupleSizes.count;
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
						var name = chunk.literals.buffer[nameIndex].asObject as string;
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