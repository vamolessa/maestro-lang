namespace Maestro.StdLib
{
	public sealed class CountCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			context.PushValue(context.inputCount);
		}
	}

	public sealed class AppendCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			for (var i = 0; i < context.inputCount; i++)
				context.PushValue(context.GetInput(i));
			context.PushValue(args.value0);
		}
	}

	public sealed class AtCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			if (!(args.value0.asObject is ValueKind.Int))
			{
				Errors.ExpectType(ref context, "argument", "int", args.value0);
				return;
			}

			var index = args.value0.asNumber.asInt;
			if (index >= 0 && index < context.inputCount)
				context.PushValue(context.GetInput(index));
			else
				context.PushValue(default);
		}
	}

	public sealed class TakeCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			if (!(args.value0.asObject is ValueKind.Int))
			{
				Errors.ExpectType(ref context, "argument", "int", args.value0);
				return;
			}

			var count = args.value0.asNumber.asInt;
			if (count <= context.inputCount)
			{
				for (var i = 0; i < count; i++)
					context.PushValue(context.GetInput(i));
			}
			else
			{
				var i = 0;
				for (; i < context.inputCount; i++)
					context.PushValue(context.GetInput(i));
				for (; i < count; i++)
					context.PushValue(default);
			}
		}
	}

	public sealed class EnumerateCommand : ICommand<Tuple2>
	{
		public void Execute(ref Context context, Tuple2 args)
		{
			if (!(args.value0.asObject is ValueKind.Int))
			{
				Errors.ExpectType(ref context, "argument 1", "int", args.value0);
				return;
			}

			if (!(args.value1.asObject is ValueKind.Int))
			{
				Errors.ExpectType(ref context, "argument 2", "int", args.value1);
				return;
			}

			var start = args.value0.asNumber.asInt;
			var count = args.value1.asNumber.asInt;
			for (var i = 0; i < count; i++)
				context.PushValue(start + i);
		}
	}
}
