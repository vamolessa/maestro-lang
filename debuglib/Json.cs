using System.Collections.Generic;

namespace Maestro.Debug
{
	public struct JsonValue
	{
		private object wrapped;

		public int Length
		{
			get { return wrapped is JsonValue[] a ? a.Length : 0; }
		}

		public JsonValue this[int index]
		{
			get { return wrapped is JsonValue[] a ? a[index] : default; }
			set { if (wrapped is JsonValue[] a) a[index] = value; }
		}

		public JsonValue this[string key]
		{
			get
			{
				return wrapped is Dictionary<string, JsonValue> d && d.TryGetValue(key, out var v) ? v : default;
			}

			set
			{
				if (wrapped is Dictionary<string, JsonValue> d)
					d[key] = value;
			}
		}
	}

	public sealed class Json
	{
	}
}
