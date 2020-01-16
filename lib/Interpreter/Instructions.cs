namespace Flow
{
	internal enum Instruction
	{
		Halt,
		ClearStack,
		LoadFalse,
		LoadTrue,
		LoadLiteral,
		ClearVariables,
		SetVariable,
		LoadVariable,
		RunCommandInstance,
	}
}