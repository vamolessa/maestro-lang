namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ClearStack,
		Pop,
		LoadNull,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		CreateArray,
		NameLocal,
		AssignLocal,
		LoadLocal,
		CallCommand,
	}
}