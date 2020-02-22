namespace Maestro.StdLib
{
	public sealed class OnlyNullsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (value.asObject is null)
					context.PushValue(value);
			}
		}
	}

	public sealed class OnlyBoolsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				switch (value.asObject)
				{
				case ValueKind.False _:
				case ValueKind.True _:
					context.PushValue(value);
					break;
				}
			}
		}
	}

	public sealed class OnlyIntsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (value.asObject is ValueKind.Int)
					context.PushValue(value);
			}
		}
	}

	public sealed class OnlyFloatsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (value.asObject is ValueKind.Float)
					context.PushValue(value);
			}
		}
	}

	public sealed class OnlyStringsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (value.asObject is string)
					context.PushValue(value);
			}
		}
	}

	public sealed class OnlyObjectsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				switch (value.asObject)
				{
				case null:
				case ValueKind.False _:
				case ValueKind.True _:
				case ValueKind.Int _:
				case ValueKind.Float _:
				case string _:
					break;
				default:
					context.PushValue(value);
					break;
				}
			}
		}
	}
}
