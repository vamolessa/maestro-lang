namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
		ExecuteCommand,
		Return,
		PushEmptyExpression,
		PopExpressionKeeping,
		PopMultiple,
		MergeTopExpression,
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