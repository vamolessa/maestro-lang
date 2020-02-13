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
				{
					var value = context.GetInput(i);
					if (!(value.asObject is null))
					{
						context.PushValue(false);
						return;
					}
				}
				break;
			case ValueKind.False _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					if (!(value.asObject is ValueKind.False))
					{
						context.PushValue(false);
						return;
					}
				}
				break;
			case ValueKind.True _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					if (!(value.asObject is ValueKind.True))
					{
						context.PushValue(false);
						return;
					}
				}
				break;
			case ValueKind.Int _:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					switch (value.asObject)
					{
					case ValueKind.Int _:
						if (args.value0.asNumber.asInt != value.asNumber.asInt)
						{
							context.PushValue(false);
							return;
						}
						break;
					case ValueKind.Float _:
						if (args.value0.asNumber.asInt != value.asNumber.asFloat)
						{
							context.PushValue(false);
							return;
						}
						break;
					default:
						context.PushValue(false);
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
						if (args.value0.asNumber.asFloat != value.asNumber.asInt)
						{
							context.PushValue(false);
							return;
						}
						break;
					case ValueKind.Float _:
						if (args.value0.asNumber.asFloat != value.asNumber.asFloat)
						{
							context.PushValue(false);
							return;
						}
						break;
					default:
						context.PushValue(false);
						return;
					}
				}
				break;
			case string argString:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					if (value.asObject is string s)
					{
						if (argString != s)
						{
							context.PushValue(false);
							return;
						}
					}
					else
					{
						context.PushValue(false);
						return;
					}
				}
				break;
			default:
				for (var i = 0; i < context.inputCount; i++)
				{
					var value = context.GetInput(i);
					if (!args.value0.Equals(value.asObject))
					{
						context.PushValue(false);
						return;
					}
				}
				break;
			}
			context.PushValue(true);
		}
	}
}
