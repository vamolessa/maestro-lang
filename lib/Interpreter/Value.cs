using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Flow
{
	public enum ValueKind
	{
		Null,
		False,
		True,
		Int,
		Float,
		Object,
		Array,
	}

	[StructLayout(LayoutKind.Explicit)]
	public readonly struct Number
	{
		[FieldOffset(0)]
		public readonly int asInt;
		[FieldOffset(0)]
		public readonly float asFloat;

		public Number(int value)
		{
			this.asFloat = default;
			this.asInt = value;
		}

		public Number(float value)
		{
			this.asInt = default;
			this.asFloat = value;
		}
	}

	public readonly struct Value
	{
		public readonly ValueKind kind;
		public readonly Number asNumber;
		public readonly object asObject;

		public Value(ValueKind kind)
		{
			this.kind = kind;
			this.asNumber = default;
			this.asObject = default;
		}

		public Value(int value)
		{
			this.kind = ValueKind.Int;
			this.asNumber = new Number(value);
			this.asObject = default;
		}

		public Value(float value)
		{
			this.kind = ValueKind.Float;
			this.asNumber = new Number(value);
			this.asObject = default;
		}

		public Value(object value)
		{
			this.kind = value != null ? ValueKind.Object : ValueKind.Null;
			this.asNumber = default;
			this.asObject = value;
		}

		public Value(Value[] value)
		{
			this.kind = value != null ? ValueKind.Array : ValueKind.Null;
			this.asNumber = default;
			this.asObject = value;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			this.AppendTo(sb);
			return sb.ToString();
		}
	}

	internal static class ValueExtensions
	{
		public static bool IsTruthy(this Value self)
		{
			switch (self.kind)
			{
			case ValueKind.Null:
			case ValueKind.False:
				return false;
			case ValueKind.True:
			case ValueKind.Object:
			case ValueKind.Array:
				return true;
			case ValueKind.Int:
				return self.asNumber.asInt != 0;
			case ValueKind.Float:
				return self.asNumber.asFloat != 0.0f;
			default:
				return false;
			}
		}

		public static bool IsEqualTo(this Value self, Value other)
		{
			if (self.kind != other.kind)
				return false;

			switch (self.kind)
			{
			case ValueKind.Null:
			case ValueKind.False:
			case ValueKind.True:
				return true;
			case ValueKind.Int:
				return self.asNumber.asInt == other.asNumber.asInt;
			case ValueKind.Float:
				return self.asNumber.asFloat == other.asNumber.asFloat;
			case ValueKind.Object:
				return self.asObject.Equals(other.asObject);
			case ValueKind.Array:
				{
					var selfArray = self.asObject as Value[];
					var otherArray = other.asObject as Value[];
					if (selfArray.Length != otherArray.Length)
						return false;

					for (var i = 0; i < selfArray.Length; i++)
					{
						if (!selfArray[i].IsEqualTo(otherArray[i]))
							return false;
					}

					return true;
				}
			default:
				return false;
			}
		}

		public static void AppendTo(this Value self, StringBuilder sb)
		{
			switch (self.kind)
			{
			case ValueKind.Null:
				sb.Append("null");
				break;
			case ValueKind.False:
				sb.Append("false");
				break;
			case ValueKind.True:
				sb.Append("true");
				break;
			case ValueKind.Int:
				sb.Append(self.asNumber.asInt);
				break;
			case ValueKind.Float:
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", self.asNumber.asFloat);
				break;
			case ValueKind.Object:
				if (self.asObject is string s)
					sb.Append('"').Append(s).Append('"');
				else
					sb.Append(self.asObject);
				break;
			case ValueKind.Array:
				{
					var array = self.asObject as Value[];
					sb.Append('[');
					for (var i = 0; i < array.Length - 1; i++)
					{
						array[i].AppendTo(sb);
						sb.Append(", ");
					}
					if (array.Length > 0)
						array[array.Length - 1].AppendTo(sb);
					sb.Append(']');
					break;
				}
			}
		}
	}
}