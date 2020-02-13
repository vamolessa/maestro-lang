namespace Maestro.StdLib
{
	public sealed class AddCommand : ICommand<Tuple1>
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
						context.PushValue(value.asNumber.asInt + args.value0.asNumber.asInt);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat + args.value0.asNumber.asInt);
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
						context.PushValue(value.asNumber.asInt + args.value0.asNumber.asFloat);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat + args.value0.asNumber.asFloat);
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

	public sealed class SubtractCommand : ICommand<Tuple1>
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
						context.PushValue(value.asNumber.asInt - args.value0.asNumber.asInt);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat - args.value0.asNumber.asInt);
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
						context.PushValue(value.asNumber.asInt - args.value0.asNumber.asFloat);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat - args.value0.asNumber.asFloat);
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

	public sealed class MultiplyCommand : ICommand<Tuple1>
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
						context.PushValue(value.asNumber.asInt * args.value0.asNumber.asInt);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat * args.value0.asNumber.asInt);
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
						context.PushValue(value.asNumber.asInt * args.value0.asNumber.asFloat);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat * args.value0.asNumber.asFloat);
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

	public sealed class DivideCommand : ICommand<Tuple1>
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
						context.PushValue(value.asNumber.asInt / args.value0.asNumber.asInt);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat / args.value0.asNumber.asInt);
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
						context.PushValue(value.asNumber.asInt / args.value0.asNumber.asFloat);
						break;
					case ValueKind.Float _:
						context.PushValue(value.asNumber.asFloat / args.value0.asNumber.asFloat);
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
}
