namespace Maestro.StdLib
{
	public sealed class LessThanCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			switch (args.value0.asObject)
			{
			case ValueKind.Int _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					switch (value.asObject)
					{
					case ValueKind.Int _:
						context.PushValue(value.asNumber.asInt < args.value0.asNumber.asInt);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat < args.value0.asNumber.asInt);
						break;
					default:
						Errors.ExpectType(ref context, "input", "int or float", value);
						return;
					}
				}
				break;
			case ValueKind.Float _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					switch (value.asObject)
					{
					case ValueKind.Int _:
						context.PushValue(value.asNumber.asInt < args.value0.asNumber.asFloat);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat < args.value0.asNumber.asFloat);
						break;
					default:
						Errors.ExpectType(ref context, "input", "int or float", value);
						return;
					}
				}
				break;
			default:
				Errors.ExpectType(ref context, "argument", "int or float", args.value0);
				break;
			}
		}
	}

	public sealed class GreaterThanCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			switch (args.value0.asObject)
			{
			case ValueKind.Int _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					switch (value.asObject)
					{
					case ValueKind.Int _:
						context.PushValue(value.asNumber.asInt > args.value0.asNumber.asInt);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat > args.value0.asNumber.asInt);
						break;
					default:
						Errors.ExpectType(ref context, "input", "int or float", value);
						return;
					}
				}
				break;
			case ValueKind.Float _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					switch (value.asObject)
					{
					case ValueKind.Int _:
						context.PushValue(value.asNumber.asInt > args.value0.asNumber.asFloat);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat > args.value0.asNumber.asFloat);
						break;
					default:
						Errors.ExpectType(ref context, "input", "int or float", value);
						return;
					}
				}
				break;
			default:
				Errors.ExpectType(ref context, "argument", "int or float", args.value0);
				break;
			}
		}
	}

	public sealed class EqualsCommand : ICommand<Tuple1>
	{
		public void Execute(ref Context context, Tuple1 args)
		{
			switch (args.value0.asObject)
			{
			case null:
				for (var i = 0; i < context.inputCount; i++)
					context.PushValue(context.GetInput(i).asObject is null);
				break;
			case ValueKind.False _:
				for (var i = 0; i < context.inputCount; i++)
					context.PushValue(context.GetInput(i).asObject is ValueKind.False);
				break;
			case ValueKind.True _:
				for (var i = 0; i < context.inputCount; i++)
					context.PushValue(context.GetInput(i).asObject is ValueKind.True);
				break;
			case ValueKind.Int _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					switch (value.asObject)
					{
					case ValueKind.Int _:
						context.PushValue(args.value0.asNumber.asInt == value.asNumber.asInt);
						break;
					case ValueKind.Float _:
						context.PushValue(args.value0.asNumber.asInt == value.asNumber.asFloat);
						break;
					default:
						context.PushValue(false);
						break;
					}
				}
				break;
			case ValueKind.Float _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					switch (value.asObject)
					{
					case ValueKind.Int _:
						context.PushValue(args.value0.asNumber.asFloat == value.asNumber.asInt);
						break;
					case ValueKind.Float _:
						context.PushValue(args.value0.asNumber.asFloat == value.asNumber.asFloat);
						break;
					default:
						context.PushValue(false);
						break;
					}
				}
				break;
			case string argString:
				for (var i = 0; i < context.inputCount; i++)
				{
					context.PushValue(
						context.GetInput(i).asObject is string s &&
						argString == s
					);
				}
				break;
			default:
				for (var i = 0; i < context.inputCount; i++)
					context.PushValue(args.value0.asObject.Equals(context.GetInput(i)));
				break;
			}
		}
	}
}
