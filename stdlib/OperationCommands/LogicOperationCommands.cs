namespace Maestro.StdLib
{
	public sealed class AndCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				if (!context.GetInput(i).IsTruthy())
				{
					context.PushValue(false);
					return;
				}
			}

			context.PushValue(true);
		}
	}

	public sealed class OrCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				if (context.GetInput(i).IsTruthy())
				{
					context.PushValue(true);
					return;
				}
			}

			context.PushValue(false);
		}
	}

	public sealed class NotCommand : ICommand<Tuple0>
	{
		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
				context.PushValue(!context.GetInput(i).IsTruthy());
		}
	}
}
