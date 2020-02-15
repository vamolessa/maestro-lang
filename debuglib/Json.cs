using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Maestro.Debug
{
	public static class Json
	{
		private static class ValueKind
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
		internal readonly struct Number
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
			internal readonly Number asNumber;
			internal readonly object asObject;

			public static Value NewArray()
			{
				return new Value(new List<Value>());
			}

			public static Value NewObject()
			{
				return new Value(new Dictionary<string, Value>());
			}

			public Value(bool value)
			{
				this.asNumber = default;
				if (value)
					this.asObject = ValueKind.TrueKind;
				else
					this.asObject = ValueKind.FalseKind;
			}

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

			public Value(string value)
			{
				this.asNumber = default;
				this.asObject = value;
			}

			private Value(object value)
			{
				this.asNumber = default;
				this.asObject = value;
			}

			public int Count
			{
				get { return asObject is List<Value> l ? l.Count : 0; }
			}

			public Value this[int index]
			{
				get { return asObject is List<Value> l ? l[index] : default; }
				set { if (asObject is List<Value> l) l[index] = value; }
			}

			public void Add(Value value)
			{
				if (asObject is List<Value> l)
					l.Add(value);
			}

			public Value this[string key]
			{
				get { return asObject is Dictionary<string, Value> d && d.TryGetValue(key, out var v) ? v : default; }
				set { if (asObject is Dictionary<string, Value> d) d[key] = value; }
			}

			public static implicit operator Value(bool value)
			{
				return new Value(value);
			}

			public static implicit operator Value(int value)
			{
				return new Value(value);
			}

			public static implicit operator Value(float value)
			{
				return new Value(value);
			}

			public static implicit operator Value(string value)
			{
				return new Value(value);
			}

			public bool IsNull
			{
				get { return asObject is null; }
			}

			public bool IsArray
			{
				get { return asObject is List<Value>; }
			}

			public bool IsObject
			{
				get { return asObject is Dictionary<string, Value>; }
			}

			public bool TryGet(out bool value)
			{
				switch (asObject)
				{
				case ValueKind.False _:
					value = false;
					return true;
				case ValueKind.True _:
					value = true;
					return true;
				default:
					value = default;
					return false;
				}
			}

			public bool TryGet(out int value)
			{
				if (asObject is ValueKind.Int)
				{
					value = asNumber.asInt;
					return true;
				}
				else
				{
					value = default;
					return false;
				}
			}

			public bool TryGet(out float value)
			{
				if (asObject is ValueKind.Float)
				{
					value = asNumber.asFloat;
					return true;
				}
				else
				{
					value = default;
					return false;
				}
			}

			public bool TryGet(out string value)
			{
				if (asObject is string s)
				{
					value = s;
					return true;
				}
				else
				{
					value = default;
					return false;
				}
			}

			public bool GetOr(bool defaultValue)
			{
				switch (asObject)
				{
				case ValueKind.False _: return false;
				case ValueKind.True _: return true;
				default: return defaultValue;
				}
			}

			public int GetOr(int defaultValue)
			{
				return asObject is ValueKind.Int ? asNumber.asInt : defaultValue;
			}

			public float GetOr(float defaultValue)
			{
				return asObject is ValueKind.Float ? asNumber.asFloat : defaultValue;
			}

			public string GetOr(string defaultValue)
			{
				return asObject is string s ? s : defaultValue;
			}
		}

		private sealed class JsonParseException : System.Exception
		{
		}

		public static void Serialize(Value value, StringBuilder sb)
		{
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
			case string s:
				sb.Append('"');
				foreach (var c in s)
				{
					switch (c)
					{
					case '\"': sb.Append("\\\""); break;
					case '\\': sb.Append("\\\\"); break;
					case '\b': sb.Append("\\b"); break;
					case '\f': sb.Append("\\f"); break;
					case '\n': sb.Append("\\n"); break;
					case '\r': sb.Append("\\r"); break;
					case '\t': sb.Append("\\t"); break;
					default: sb.Append(c); break;
					}
				}
				sb.Append('"');
				break;
			case List<Value> l:
				sb.Append('[');
				foreach (var v in l)
				{
					Serialize(v, sb);
					sb.Append(',');
				}
				if (l.Count > 0)
					sb.Remove(sb.Length - 1, 1);
				sb.Append(']');
				break;
			case Dictionary<string, Value> d:
				sb.Append('{');
				foreach (var p in d)
				{
					sb.Append('"').Append(p.Key).Append('"').Append(':');
					Serialize(p.Value, sb);
					sb.Append(',');
				}
				if (d.Count > 0)
					sb.Remove(sb.Length - 1, 1);
				sb.Append('}');
				break;
			}
		}

		public static bool TryDeserialize(string source, out Value value)
		{
			try
			{
				var index = 0;
				value = Parse(source, ref index, new StringBuilder());
				return true;
			}
			catch (JsonParseException)
			{
				value = default;
				return false;
			}
		}

		private static Value Parse(string source, ref int index, StringBuilder sb)
		{
			SkipWhiteSpace(source, ref index);
			switch (Next(source, ref index))
			{
			case 'n':
				Consume(source, ref index, 'u');
				Consume(source, ref index, 'l');
				Consume(source, ref index, 'l');
				SkipWhiteSpace(source, ref index);
				return new Value(null);
			case 'f':
				Consume(source, ref index, 'a');
				Consume(source, ref index, 'l');
				Consume(source, ref index, 's');
				Consume(source, ref index, 'e');
				SkipWhiteSpace(source, ref index);
				return new Value(false);
			case 't':
				Consume(source, ref index, 'r');
				Consume(source, ref index, 'u');
				Consume(source, ref index, 'e');
				SkipWhiteSpace(source, ref index);
				return new Value(true);
			case '"':
				return ConsumeString(source, ref index, sb);
			case '[':
				{
					SkipWhiteSpace(source, ref index);
					var array = Value.NewArray();
					if (!Match(source, ref index, ']'))
					{
						do
						{
							var value = Parse(source, ref index, sb);
							array.Add(value);
						} while (Match(source, ref index, ','));
						Consume(source, ref index, ']');
					}
					SkipWhiteSpace(source, ref index);
					return array;
				}
			case '{':
				{
					SkipWhiteSpace(source, ref index);
					var obj = Value.NewObject();
					if (!Match(source, ref index, '}'))
					{
						do
						{
							SkipWhiteSpace(source, ref index);
							Consume(source, ref index, '"');
							var key = ConsumeString(source, ref index, sb);
							Consume(source, ref index, ':');
							var value = Parse(source, ref index, sb);
							obj[key.asObject as string] = value;
						} while (Match(source, ref index, ','));
						Consume(source, ref index, '}');
					}
					SkipWhiteSpace(source, ref index);
					return obj;
				}
			default:
				{
					bool IsDigit(string s, int i)
					{
						return i < s.Length && char.IsDigit(s, i);
					}

					var negative = source[--index] == '-';
					if (negative)
						index++;
					if (!IsDigit(source, index))
						throw new JsonParseException();

					while (Match(source, ref index, '0'))
						continue;

					var integer = 0;
					while (IsDigit(source, index))
					{
						integer = 10 * integer + source[index] - '0';
						index++;
					}

					if (Match(source, ref index, '.'))
					{
						if (!IsDigit(source, index))
							throw new JsonParseException();

						var fractionBase = 1.0f;
						var fraction = 0.0f;

						while (IsDigit(source, index))
						{
							fractionBase *= 0.1f;
							fraction += (source[index] - '0') * fractionBase;
							index++;
						}

						fraction += integer;
						SkipWhiteSpace(source, ref index);
						return new Value(negative ? -fraction : fraction);
					}

					SkipWhiteSpace(source, ref index);
					return new Value(negative ? -integer : integer);
				}
			}
		}

		private static void SkipWhiteSpace(string source, ref int index)
		{
			while (index < source.Length && char.IsWhiteSpace(source, index))
				index++;
		}

		private static char Next(string source, ref int index)
		{
			if (index < source.Length)
				return source[index++];
			throw new JsonParseException();
		}

		private static bool Match(string source, ref int index, char c)
		{
			if (index < source.Length && source[index] == c)
			{
				index++;
				return true;
			}
			else
			{
				return false;
			}
		}

		private static void Consume(string source, ref int index, char c)
		{
			if (index >= source.Length || source[index++] != c)
				throw new JsonParseException();
		}

		private static Value ConsumeString(string source, ref int index, StringBuilder sb)
		{
			sb.Clear();
			while (index < source.Length)
			{
				var c = Next(source, ref index);
				switch (c)
				{
				case '"':
					SkipWhiteSpace(source, ref index);
					return new Value(sb.ToString());
				case '\\':
					switch (Next(source, ref index))
					{
					case '"': sb.Append('"'); break;
					case '\\': sb.Append('\\'); break;
					case '/': sb.Append('/'); break;
					case 'b': sb.Append('\b'); break;
					case 'f': sb.Append('\f'); break;
					case 'n': sb.Append('\n'); break;
					case 'r': sb.Append('\r'); break;
					case 't': sb.Append('\t'); break;
					case 'u': throw new JsonParseException();
					default: throw new JsonParseException();
					}
					break;
				default:
					sb.Append(c);
					break;
				}
			}
			throw new JsonParseException();
		}
	}
}