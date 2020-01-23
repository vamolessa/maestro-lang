namespace Flow
{
	internal enum Instruction
	{
		Halt,
		CallNativeCommand,
		Pop,
		PopMultiple,
		LoadNull,
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