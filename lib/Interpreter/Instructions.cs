namespace Flow
{
	internal enum Instruction
	{
		Halt,
		CallNativeCommand,
		ClearStack,
		Pop,
		LoadNull,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		CreateArray,
		AddLocalName,
		AssignLocal,
		LoadLocal,
	}
}