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
			var expressionSizes = new Buffer<int>(8);

			void PopExpression()
			{
				var count = expressionSizes.buffer[--expressionSizes.count];
				while (count-- > 0)
					stack.buffer[--stack.count] = default;
			}

			void Keep(byte keepCount)
			{
				var diff = keepCount - expressionSizes.buffer[expressionSizes.count - 1];
				expressionSizes.buffer[expressionSizes.count - 1] = keepCount;

				if (diff > 0)
				{
					stack.GrowUnchecked(diff);
					return;
				}

				while (diff++ < 0)
					stack.buffer[--stack.count] = default;
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
						context.inputCount = expressionSizes.PopLast();

						var previousStackCount = stack.count;
						context.startIndex = stack.count - definition.parameterCount - context.inputCount;

						command.Invoke(ref context);
						stack = context.stack;

						var returnCount = stack.count - previousStackCount;
						expressionSizes.PushBackUnchecked(returnCount);

						while (returnCount-- > 0)
							stack.buffer[context.startIndex++] = stack.buffer[previousStackCount + returnCount];

						previousStackCount = stack.count;
						stack.count = context.startIndex;

						while (context.startIndex < previousStackCount)
							stack.buffer[context.startIndex++] = default;

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

						baseStackIndex = stack.count - 1 - definition.parameterCount;

						vm.stackFrames.PushBackUnchecked(new StackFrame(codeIndex, baseStackIndex, index));
						break;
					}
				case Instruction.Return:
					{
						var count = bytes[codeIndex++];

						var frame = vm.stackFrames.buffer[--vm.stackFrames.count];
						var instance = vm.chunk.commandInstances.buffer[frame.commandInstanceIndex];
						var definition = vm.chunk.commandDefinitions.buffer[instance.definitionIndex];

						var inputCount = stack.buffer[frame.baseStackIndex].asNumber.asInt;
						stack.count = frame.baseStackIndex - inputCount;
						var returnStartIndex = frame.baseStackIndex + 1 + definition.parameterCount;

						for (var i = 0; i < count; i++)
							stack.buffer[stack.count++] = stack.buffer[returnStartIndex++];

						while (returnStartIndex > stack.count)
							stack.buffer[--returnStartIndex] = default;

						frame = vm.stackFrames.buffer[vm.stackFrames.count - 1];
						codeIndex = frame.codeIndex;
						baseStackIndex = frame.baseStackIndex;
						break;
					}
				case Instruction.PushEmptyExpression:
					expressionSizes.PushBackUnchecked(0);
					break;
				case Instruction.PopExpression:
					PopExpression();
					break;
				case Instruction.PopExpressionKeepOne:
					Keep(1);
					break;
				case Instruction.PopExpressionKeepMultiple:
					Keep(bytes[codeIndex++]);
					break;
				case Instruction.PopMultiple:
					{
						var count = bytes[codeIndex++];
						while (count-- > 0)
							stack.buffer[--stack.count] = default;
						break;
					}
				case Instruction.AppendExpression:
					expressionSizes.buffer[expressionSizes.count - 2] = expressionSizes.buffer[--expressionSizes.count];
					break;
				case Instruction.LoadFalse:
					stack.PushBackUnchecked(new Value(ValueKind.FalseKind));
					expressionSizes.PushBackUnchecked(1);
					break;
				case Instruction.LoadTrue:
					stack.PushBackUnchecked(new Value(ValueKind.TrueKind));
					expressionSizes.PushBackUnchecked(1);
					break;
				case Instruction.LoadLiteral:
					{
						var index = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						stack.PushBackUnchecked(vm.chunk.literals.buffer[index]);
						expressionSizes.PushBackUnchecked(1);
						break;
					}
				case Instruction.CreateLocals:
					Keep(bytes[codeIndex++]);
					expressionSizes.buffer[expressionSizes.count - 1] = 0;
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
						expressionSizes.PushBackUnchecked(1);
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

						for (var i = stack.count - expressionSizes.buffer[expressionSizes.count - 1]; i < stack.count; i++)
						{
							if (!stack.buffer[i].IsTruthy())
							{
								codeIndex += offset;
								break;
							}
						}

						PopExpression();
						break;
					}
				case Instruction.JumpForwardIfExpressionIsEmptyKeepingOne:
					{
						var offset = BytesHelper.BytesToUShort(bytes[codeIndex++], bytes[codeIndex++]);
						if (expressionSizes.buffer[expressionSizes.count - 1] == 0)
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