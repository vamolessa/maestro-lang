using System.Globalization;

namespace Maestro
{
	internal enum VariableFlag
	{
		NotRead,
		Unwritten,
		Fulfilled,
		Input,
	}

	internal struct Variable
	{
		public Slice slice;
		public VariableFlag flag;

		public Variable(Slice slice, VariableFlag flag)
		{
			this.slice = slice;
			this.flag = flag;
		}

		public void PerformedRead()
		{
			if (flag == VariableFlag.NotRead)
				flag = VariableFlag.Fulfilled;
		}

		public void PerformedWrite()
		{
			if (flag == VariableFlag.Unwritten)
				flag = VariableFlag.Fulfilled;
		}
	}

	internal enum ScopeType
	{
		Normal,
		IterationBody,
		CommandBody,
	}

	internal readonly struct Scope
	{
		public readonly ScopeType type;
		public readonly int variablesStartIndex;

		public Scope(ScopeType type, int variablesStartIndex)
		{
			this.type = type;
			this.variablesStartIndex = variablesStartIndex;
		}
	}

	internal static class CompilerHelper
	{
		public static void ConsumeSemicolon<T>(Compiler compiler, Slice slice, T error) where T : IFormattedMessage
		{
			compiler.parser.Next();
			if (compiler.parser.previousToken.kind != TokenKind.SemiColon)
				compiler.AddHardError(slice, error);
		}

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