using System.Text;

namespace Flow
{
	internal static class ValueHelper
	{
		public static void ValueToString(object value, StringBuilder sb)
		{
			if (value == null)
			{
				sb.Append("null");
			}
			else if (value is string text)
			{
				sb.Append('"');
				sb.Append(text);
				sb.Append('"');
			}
			else if (value is object[] array)
			{
				sb.Append('[');
				for (var i = 0; i < array.Length - 1; i++)
				{
					ValueToString(array[i], sb);
					sb.Append(", ");
				}
				if (array.Length > 0)
					ValueToString(array[array.Length - 1], sb);
				sb.Append(']');
			}
			else
			{
				sb.Append(value);
			}
		}
	}
}