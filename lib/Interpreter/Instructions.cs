namespace Flow
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
		JumpBackward,
		JumpForward,
		PopExpressionAndJumpForwardIfAnyFalse,
		JumpForwardIfExpressionIsEmptyKeepingOne,

		DebugHook,
		DebugPushLocalInfo,
		DebugPopLocalInfos,
	}
}