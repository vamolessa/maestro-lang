using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Maestro.Debug
{
	public readonly struct JsonValue
	{
		public readonly object wrapped;

		public static JsonValue NewArray()
		{
			return new JsonValue(new List<JsonValue>());
		}

		public static JsonValue NewObject()
		{
			return new JsonValue(new Dictionary<string, JsonValue>());
		}

		public JsonValue(object value)
		{
			wrapped = value;
		}

		public int Count
		{
			get { return wrapped is List<JsonValue> l ? l.Count : 0; }
		}

		public JsonValue this[int index]
		{
			get { return wrapped is List<JsonValue> l ? l[index] : default; }
			set { if (wrapped is List<JsonValue> l) l[index] = value; }
		}

		public void Add(JsonValue value)
		{
			if (wrapped is List<JsonValue> l)
				l.Add(value);
		}

		public JsonValue this[string key]
		{
			get { return wrapped is Dictionary<string, JsonValue> d && d.TryGetValue(key, out var v) ? v : default; }
			set { if (wrapped is Dictionary<string, JsonValue> d) d[key] = value; }
		}

		public static implicit operator JsonValue(bool value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(int value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(float value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(string value)
		{
			return new JsonValue(value);
		}

		public bool IsArray
		{
			get { return wrapped is List<JsonValue>; }
		}

		public bool IsObject
		{
			get { return wrapped is Dictionary<string, JsonValue>; }
		}

		public bool TryGet(out bool value)
		{
			if (wrapped is bool v)
			{
				value = v;
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}

		public bool TryGet(out int value)
		{
			if (wrapped is int v)
			{
				value = v;
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
			if (wrapped is float v)
			{
				value = v;
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
			if (wrapped is string v)
			{
				value = v;
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}
	}

	public static class Json
	{
		private sealed class JsonParseException : System.Exception
		{
		}

		public static void Serialize(JsonValue value, StringBuilder sb)
		{
			switch (value.wrapped)
			{
			case bool b:
				sb.Append(b ? "true" : "false");
				break;
			case int i:
				sb.Append(i);
				break;
			case float f:
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", f);
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
			case List<JsonValue> l:
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
			case Dictionary<string, JsonValue> d:
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
			default:
				sb.Append("null");
				break;
			}
		}

		public static bool TryDeserialize(string source, out JsonValue value)
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

		private static JsonValue Parse(string source, ref int index, StringBuilder sb)
		{
			SkipWhiteSpace(source, ref index);
			switch (Next(source, ref index))
			{
			case 'n':
				Consume(source, ref index, 'u');
				Consume(source, ref index, 'l');
				Consume(source, ref index, 'l');
				SkipWhiteSpace(source, ref index);
				return new JsonValue(null);
			case 'f':
				Consume(source, ref index, 'a');
				Consume(source, ref index, 'l');
				Consume(source, ref index, 's');
				Consume(source, ref index, 'e');
				SkipWhiteSpace(source, ref index);
				return new JsonValue(false);
			case 't':
				Consume(source, ref index, 'r');
				Consume(source, ref index, 'u');
				Consume(source, ref index, 'e');
				SkipWhiteSpace(source, ref index);
				return new JsonValue(true);
			case '"':
				return ConsumeString(source, ref index, sb);
			case '[':
				{
					SkipWhiteSpace(source, ref index);
					var array = JsonValue.NewArray();
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
					var obj = JsonValue.NewObject();
					if (!Match(source, ref index, '}'))
					{
						do
						{
							SkipWhiteSpace(source, ref index);
							Consume(source, ref index, '"');
							var key = ConsumeString(source, ref index, sb);
							Consume(source, ref index, ':');
							var value = Parse(source, ref index, sb);
							obj[key.wrapped as string] = value;
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
						return new JsonValue(negative ? -fraction : fraction);
					}

					SkipWhiteSpace(source, ref index);
					return new JsonValue(negative ? -integer : integer);
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

		private static JsonValue ConsumeString(string source, ref int index, StringBuilder sb)
		{
			sb.Clear();
			while (index < source.Length)
			{
				var c = Next(source, ref index);
				switch (c)
				{
				case '"':
					SkipWhiteSpace(source, ref index);
					return new JsonValue(sb.ToString());
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