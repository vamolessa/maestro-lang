namespace Maestro.StdLib
{
	public sealed class IsNullCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			context.PushValue(args.value0.asObject is null);
		}
	}

	public sealed class IsBoolCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			context.PushValue(
				args.value0.asObject is ValueKind.False ||
				args.value0.asObject is ValueKind.True
			);
		}
	}

	public sealed class IsIntCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			context.PushValue(args.value0.asObject is ValueKind.Int);
		}
	}

	public sealed class IsFloatCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			context.PushValue(args.value0.asObject is ValueKind.Float);
		}
	}

	public sealed class IsStringCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			context.PushValue(args.value0.asObject is string);
		}
	}

	public sealed class IsObjectCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			switch (args.value0.asObject)
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
