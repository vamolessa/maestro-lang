namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ClearVariables,
		Pop,
		LoadNull,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		CreateArray,
		PipeVariable,
		LoadVariable,
		RunCommandInstance,
	}
}