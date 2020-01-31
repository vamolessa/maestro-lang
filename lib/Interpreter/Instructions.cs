namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
		ExecuteCommand,
		Return,
		PushExpressionSize,
		PopOne,
		PopMultiple,
		PopUnknown,
		KeepOne,
		KeepMultiple,
		AppendBottomUnknown,
		AppendTopUnknown,
		AppendBothUnkown,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		AssignLocal,
		LoadLocal,
		JumpBackward,
		JumpForward,
		PopAndJumpForwardIfFalse,
		JumpForwardIfNull,

		DebugHook,
		DebugPushLocalInfo,
		DebugPopLocalInfos,
	}
}