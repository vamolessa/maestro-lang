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
		AddLocalName,
		AssignLocal,
		LoadLocal,
		PopLocals,
		JumpForward,
		PopAndJumpForwardIfFalse,
	}
}