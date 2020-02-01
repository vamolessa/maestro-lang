namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
		ExecuteCommand,
		Return,
		PushEmptyExpression,
		PopOneExpression,
		PopMultipleExpressions,
		PopExpressionKeepOne,
		PopExpressionKeepMultiple,
		AppendExpression,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		CreateLocals,
		AssignLocal,
		LoadLocal,
		JumpBackward,
		JumpForward,
		PopExpressionAndJumpForwardIfFalse,
		JumpForwardIfExpressionIsEmptyKeepingOne,

		DebugHook,
		DebugPushLocalInfo,
		DebugPopLocalInfos,
	}
}