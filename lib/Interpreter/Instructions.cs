namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
		ExecuteCommand,
		Return,
		PushEmptyExpression,
		PopExpression,
		PopExpressionKeepOne,
		PopExpressionKeepMultiple,
		PopMultiple,
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