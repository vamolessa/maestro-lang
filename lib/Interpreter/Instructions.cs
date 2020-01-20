namespace Flow
{
	internal enum Instruction
	{
		Halt,
		Pop,
		LoadNull,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		CreateArray,
		AssignLocal,
		LoadLocal,
		RunCommandInstance,
	}
}