using System.Globalization;
using System.Text;

namespace Maestro.StdLib
{
	public sealed class PrintCommand : ICommand<Tuple0>
	{
		private readonly StringBuilder sb = new StringBuilder();

		public void Execute(ref Context context, Tuple0 args)
		{
			for (var i = 0; i < context.inputCount; i++)
			{
				var value = context.GetInput(i);
				switch (value.asObject)
				{
				case null:
					sb.Append("null");
					break;
				case ValueKind.False _:
					sb.Append("false");
					break;
				case ValueKind.True _:
					sb.Append("true");
					break;
				case ValueKind.Int _:
					sb.Append(value.asNumber.asInt);
					break;
				case ValueKind.Float _:
					sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", value.asNumber.asFloat);
					break;
				default:
					sb.Append(value.asObject);
					break;
				}

				sb.Append(' ');
			}
			System.Console.WriteLine(sb);
			sb.Clear();
		}
	}
}
