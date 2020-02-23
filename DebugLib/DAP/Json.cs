using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Maestro.Debug
{
	public static class Json
	{
		public struct Array : IEnumerable
		{
			internal List<Value> collection;

			public void Add(Value value)
			{
				if (collection == null)
					collection = new List<Value>();
				collection.Add(value);
			}

			public IEnumerator GetEnumerator()
			{
				if (collection == null)
					collection = new List<Value>();
				return collection.GetEnumerator();
			}
		}

		public struct Object : IEnumerable
		{
			internal Dictionary<string, Value> collection;

			public void Add(string key, Value value)
			{
				if (collection == null)
					collection = new Dictionary<string, Value>();
				collection.Add(key, value);
			}

			public IEnumerator GetEnumerator()
			{
				if (collection == null)
					collection = new Dictionary<string, Value>();
				return collection.GetEnumerator();
			}
		}

		public readonly struct Value
		{
			private static readonly List<Value> EmptyValues = new List<Value>();

			public readonly object wrapped;

			private Value(object value)
			{
				wrapped = value;
			}

			public int Count
			{
				get { return wrapped is List<Value> l ? l.Count : 0; }
			}

			public Value this[int index]
			{
				get { return wrapped is List<Value> l ? l[index] : default; }
				set { if (wrapped is List<Value> l) l[index] = value; }
			}

			public void Add(Value value)
			{
				if (wrapped is List<Value> l)
					l.Add(value);
			}

			public Value this[string key]
			{
				get { return wrapped is Dictionary<string, Value> d && d.TryGetValue(key, out var v) ? v : default; }
				set { if (wrapped is Dictionary<string, Value> d) d[key] = value; }
			}

			public static implicit operator Value(Array value)
			{
				return new Value(value.collection ?? new List<Value>());
			}

			public static implicit operator Value(Object value)
			{
				return new Value(value.collection ?? new Dictionary<string, Value>());
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

			public bool IsArray
			{
				get { return wrapped is List<Value>; }
			}

			public bool IsObject
			{
				get { return wrapped is Dictionary<string, Value>; }
			}

			public bool TryGet<T>(out T value)
			{
				if (wrapped is T v)
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

			public T GetOr<T>(T defaultValue)
			{
				return wrapped is T value ? value : defaultValue;
			}

			public List<Value>.Enumerator GetEnumerator()
			{
				return wrapped is List<Value> l ?
					l.GetEnumerator() :
					EmptyValues.GetEnumerator();
			}
		}

		private sealed class JsonParseException : System.Exception
		{
		}

		public static string Serialize(Value value)
		{
			var sb = new StringBuilder();
			Serialize(value, sb);
			return sb.ToString();
		}

		public static void Serialize(Value value, StringBuilder sb)
		{
			switch (value.wrapped)
			{
			case null:
				sb.Append("null");
				break;
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
				return new Value();
			case 'f':
				Consume(source, ref index, 'a');
				Consume(source, ref index, 'l');
				Consume(source, ref index, 's');
				Consume(source, ref index, 'e');
				SkipWhiteSpace(source, ref index);
				return false;
			case 't':
				Consume(source, ref index, 'r');
				Consume(source, ref index, 'u');
				Consume(source, ref index, 'e');
				SkipWhiteSpace(source, ref index);
				return true;
			case '"':
				return ConsumeString(source, ref index, sb);
			case '[':
				{
					SkipWhiteSpace(source, ref index);
					var array = new Array();
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
					var obj = new Object();
					if (!Match(source, ref index, '}'))
					{
						do
						{
							SkipWhiteSpace(source, ref index);
							Consume(source, ref index, '"');
							var key = ConsumeString(source, ref index, sb);
							Consume(source, ref index, ':');
							var value = Parse(source, ref index, sb);
							obj.Add(key.wrapped as string, value);
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
						return negative ? -fraction : fraction;
					}

					SkipWhiteSpace(source, ref index);
					return negative ? -integer : integer;
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
					return sb.ToString();
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