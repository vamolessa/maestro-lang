namespace Maestro.StdLib
{
	public sealed class RemoveNullsCommand : ICommand<Tuple0>
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

	public sealed class RemoveBoolsCommand : ICommand<Tuple0>
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

	public sealed class RemoveIntsCommand : ICommand<Tuple0>
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

	public sealed class RemoveFloatsCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (!(value.asObject is ValueKind.Float))
					context.PushValue(value);
			}
		}
	}

	public sealed class RemoveStringsCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				if (!(value.asObject is string))
					context.PushValue(value);
			}
		}
	}

	public sealed class RemoveObjectsCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
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
