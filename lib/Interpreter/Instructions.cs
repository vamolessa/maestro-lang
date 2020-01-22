namespace Flow
{
	internal enum Instruction
	{
		Halt,
		CallNativeCommand,
		Pop,
		LoadNull,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		CreateArray,
		AddLocalName,
		AssignLocal,
		LoadLocal,
		PopLocals,
		JumpForward,
		PopAndJumpForwardIfFalse,
	}
}