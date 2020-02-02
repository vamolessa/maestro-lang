namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
		ExecuteCommand,
		Return,
		PushEmptyExpression,
		PopExpressionKeepingValues,
		PopMultiple,
		AppendExpression,
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