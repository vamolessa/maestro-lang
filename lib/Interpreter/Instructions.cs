namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
		ExecuteCommand,
		Return,
		Pop,
		PopMultiple,
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