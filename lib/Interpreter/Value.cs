using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Maestro
{
	public static class ValueKind
	{
		public sealed class False { }
		public sealed class True { }
		public sealed class Int { }
		public sealed class Float { }

		public static readonly False FalseKind = new False();
		public static readonly True TrueKind = new True();
		public static readonly Int IntKind = new Int();
		public static readonly Float FloatKind = new Float();
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

	[DebuggerTypeProxy(typeof(ValueDebugView))]
	public readonly struct Value
	{
		public readonly Number asNumber;
		public readonly object asObject;

		public Value(int value)
		{
			this.asNumber = new Number(value);
			this.asObject = ValueKind.IntKind;
		}

		public Value(float value)
		{
			this.asNumber = new Number(value);
			this.asObject = ValueKind.FloatKind;
		}

		public Value(object value)
		{
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

	public static class ValueExtensions
	{
		public static bool IsTruthy(this Value self)
		{
			switch (self.asObject)
			{
			case null:
			case ValueKind.False _:
				return false;
			case ValueKind.Int _:
				return self.asNumber.asInt != 0;
			case ValueKind.Float _:
				return self.asNumber.asFloat != 0.0f;
			case string s:
				return s.Length != 0;
			default:
				return true;
			}
		}

		public static bool IsEqualTo(this Value self, Value other)
		{
			switch (self.asObject)
			{
			case null:
				return other.asObject is null;
			case ValueKind.False _:
				return other.asObject is ValueKind.False;
			case ValueKind.True _:
				return other.asObject is ValueKind.True;
			case ValueKind.Int _:
				return other.asObject is ValueKind.Int _ && self.asNumber.asInt == other.asNumber.asInt;
			case ValueKind.Float _:
				return other.asObject is ValueKind.Float _ && self.asNumber.asFloat == other.asNumber.asFloat;
			default:
				return self.asObject.Equals(other.asObject);
			}
		}

		public static void AppendTo(this Value self, StringBuilder sb)
		{
			switch (self.asObject)
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
				sb.Append(self.asNumber.asInt);
				break;
			case ValueKind.Float _:
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0:0.00}", self.asNumber.asFloat);
				break;
			case string s:
				sb.Append('"').Append(s).Append('"');
				break;
			default:
				sb.Append(self.asObject);
				break;
			}
		}
	}

	internal sealed class ValueDebugView
	{
		internal readonly string formatted;

		internal ValueDebugView(Value value)
		{
			var sb = new StringBuilder();
			value.AppendTo(sb);
			formatted = sb.ToString();
		}
	}
}