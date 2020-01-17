using System.Text;

namespace Flow
{
	public static class Program
	{
		public static string GetFormattedCompileErrors(Buffer<CompileError> errors, Source source)
		{
			var sb = new StringBuilder();

			for (var i = 0; i < errors.count; i++)
			{
				var e = errors.buffer[i];
				sb.Append(e.message);

				if (e.slice.index > 0 || e.slice.length > 0)
				{
					FormattingHelper.AddHighlightSlice(source.uri.value, source.content, e.slice, sb);
				}
			}

			return sb.ToString();
		}

		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.flow");
			var source = new Source(new Uri("script.flow"), content);

			var chunk = new ByteCodeChunk();
			chunk.RegisterCommand(new Command("command", null));
			chunk.RegisterCommand(new Command("print", null));

			var compiler = new Compiler();
			var errors = compiler.CompileSource(chunk, source);

			if (errors.count > 0)
			{
				var formattedErrors = GetFormattedCompileErrors(errors, source);
				System.Console.WriteLine(formattedErrors);
			}
			else
			{
				var sb = new StringBuilder();
				chunk.Disassemble(sb);
				System.Console.WriteLine(sb);
			}
		}
	}
}
