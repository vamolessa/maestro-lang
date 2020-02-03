namespace Maestro
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
		ExecuteCommand,
		Return,
		PushEmptyTuple,
		PopTupleKeeping,
		MergeTuple,
		Pop,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		AssignLocal,
		LoadLocal,
		LoadInput,
		JumpBackward,
		JumpForward,
		IfConditionJump,
		IterateConditionJump,

		DebugHook,
		DebugPushDebugFrame,
		DebugPopDebugFrame,
		DebugPushVariableInfo,
	}
}