using System.Globalization;

namespace Flow
{
	internal static class CompilerHelper
	{
		public static bool AreEqual(string source, Slice a, Slice b)
		{
			if (a.length != b.length)
				return false;

			for (var i = 0; i < a.length; i++)
			{
				if (source[a.index + i] != source[b.index + i])
					return false;
			}

			return true;
		}

		public static bool AreEqual(string source, Slice slice, string other)
		{
			if (slice.length != other.Length)
				return false;

			for (var i = 0; i < slice.length; i++)
			{
				if (source[slice.index + i] != other[i])
					return false;
			}

			return true;
		}

		public static string GetSlice(CompilerIO io, Slice slice)
		{
			return io.parser.tokenizer.source.Substring(slice.index, slice.length);
		}

		public static int GetParsedInt(CompilerIO io)
		{
			var sub = GetSlice(io, io.parser.previousToken.slice);
			int.TryParse(sub, out var value);
			return value;
		}

		public static float GetParsedFloat(CompilerIO io)
		{
			var sub = GetSlice(io, io.parser.previousToken.slice);
			float.TryParse(
				sub,
				NumberStyles.Float,
				CultureInfo.InvariantCulture.NumberFormat,
				out var value);
			return value;
		}

		public static string GetParsedString(CompilerIO io)
		{
			var slice = new Slice(
				io.parser.previousToken.slice.index + 1,
				io.parser.previousToken.slice.length - 2
			);
			return GetSlice(io, slice);
		}
	}
}