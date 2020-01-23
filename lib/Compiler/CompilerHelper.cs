using System.Globalization;

namespace Flow
{
	internal readonly struct ExpressionResult
	{
		public readonly Slice slice;
		public readonly byte valueCount;

		public ExpressionResult(Slice slice, byte count)
		{
			this.slice = slice;
			this.valueCount = count;
		}
	}

	internal struct LocalVariable
	{
		public Slice slice;
		public bool used;

		public LocalVariable(Slice slice, bool used)
		{
			this.slice = slice;
			this.used = used;
		}
	}

	internal readonly struct Scope
	{
		public readonly int localVariablesStartIndex;

		public Scope(int localVarStartIndex)
		{
			this.localVariablesStartIndex = localVarStartIndex;
		}
	}

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

		public static string GetSlice(Compiler compiler, Slice slice)
		{
			return compiler.parser.tokenizer.source.Substring(slice.index, slice.length);
		}

		public static int GetParsedInt(Compiler compiler)
		{
			var sub = GetSlice(compiler, compiler.parser.previousToken.slice);
			int.TryParse(sub, out var value);
			return value;
		}

		public static float GetParsedFloat(Compiler compiler)
		{
			var sub = GetSlice(compiler, compiler.parser.previousToken.slice);
			float.TryParse(
				sub,
				NumberStyles.Float,
				CultureInfo.InvariantCulture.NumberFormat,
				out var value);
			return value;
		}

		public static string GetParsedString(Compiler compiler)
		{
			var slice = new Slice(
				compiler.parser.previousToken.slice.index + 1,
				compiler.parser.previousToken.slice.length - 2
			);
			return GetSlice(compiler, slice);
		}
	}
}