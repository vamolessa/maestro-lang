namespace Maestro.StdLib
{
	public sealed class ExceptNullsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (!(value.asObject is null))
					context.PushValue(value);
			}
		}
	}

	public sealed class ExceptBoolsCommand : ICommand<Tuple0>
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
					break;
				default:
					context.PushValue(value);
					break;
				}
			}
		}
	}

	public sealed class ExceptIntsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (!(value.asObject is ValueKind.Int))
					context.PushValue(value);
			}
		}
	}

	public sealed class ExceptFloatsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (!(value.asObject is ValueKind.Float))
					context.PushValue(value);
			}
		}
	}

	public sealed class ExceptStringsCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (!(value.asObject is string))
					context.PushValue(value);
			}
		}
	}

	public sealed class ExceptObjectsCommand : ICommand<Tuple0>
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
					context.PushValue(value);
					break;
				}
			}
		}
	}
}
