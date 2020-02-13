namespace Maestro.StdLib
{
	public sealed class IsNullCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
				context.PushValue(context.GetInput(i).asObject is null);
		}
	}

	public sealed class IsBoolCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				context.PushValue(value.asObject is ValueKind.False || value.asObject is ValueKind.True);
			}
		}
	}

	public sealed class IsIntCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
				context.PushValue(context.GetInput(i).asObject is ValueKind.Int);
		}
	}

	public sealed class IsFloatCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
				context.PushValue(context.GetInput(i).asObject is ValueKind.Float);
		}
	}

	public sealed class IsStringCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
				context.PushValue(context.GetInput(i).asObject is string);
		}
	}

	public sealed class IsObjectCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				switch (context.GetInput(i).asObject)
				{
				case null:
				case ValueKind.False _:
				case ValueKind.True _:
				case ValueKind.Int _:
				case ValueKind.Float _:
				case string _:
					context.PushValue(false);
					break;
				default:
					context.PushValue(true);
					break;
				}
			}
		}
	}
}
