namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
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