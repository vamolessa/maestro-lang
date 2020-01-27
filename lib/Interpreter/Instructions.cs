namespace Flow
{
	internal enum Instruction
	{
		Halt,
		CallNativeCommand,
		Pop,
		PopMultiple,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		PushLocalInfo,
		PopLocalInfos,
		AssignLocal,
		LoadLocal,
		JumpForward,
		PopAndJumpForwardIfFalse,
	}
}