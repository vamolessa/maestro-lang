namespace Maestro
{
	internal enum Instruction
	{
		Halt,
		ExecuteNativeCommand,
		ExecuteCommand,
		ExecuteExternalCommand,
		Return,
		PushEmptyTuple,
		PopTupleKeeping,
		MergeTuple,
		Pop,
		PushFalse,
		PushTrue,
		PushLiteral,
		SetLocal,
		PushLocal,
		PushInput,
		JumpBackward,
		JumpForward,
		IfConditionJump,
		ForEachConditionJump,

		DebugHook,
		DebugPushDebugFrame,
		DebugPopDebugFrame,
		DebugPushVariableInfo,
		DebugPopVariableInfo,
	}
}