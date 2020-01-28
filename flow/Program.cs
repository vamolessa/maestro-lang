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
				sb.Append(e.message.Format());

				if (e.slice.index > 0 || e.slice.length > 0)
				{
					FormattingHelper.AddHighlightSlice(source.uri.value, source.content, e.slice, sb);
				}
			}

			return sb.ToString();
		}

		public sealed class PrintCommand : ICommand<Tuple0, Tuple0>
		{
			public Tuple0 Invoke(Inputs inputs, Tuple0 args)
			{
				System.Console.WriteLine($"PRINTING {inputs.count} INPUTS:");
				for (var i = 0; i < inputs.count; i++)
					System.Console.WriteLine(inputs[i].ToString());

				return default;
			}
		}

		public sealed class BypassCommand : ICommand<Tuple0, Tuple1>
		{
			public Tuple1 Invoke(Inputs inputs, Tuple0 args)
			{
				System.Console.WriteLine($"BYPASS WITH {inputs.count} INPUTS");
				return inputs.count > 0 ? inputs[0] : new Value(null);
			}
		}

		public sealed class ElementsCommand : ICommand<Tuple0, Tuple1>
		{
			public int currentIndex = 0;

			public Tuple1 Invoke(Inputs inputs, Tuple0 args)
			{
				if (currentIndex < inputs.count)
				{
					return inputs[currentIndex++];
				}
				else
				{
					currentIndex = 0;
					return default;
				}
			}
		}

		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.flow");
			var source = new Source(new Uri("script.flow"), content);

			var engine = new Engine();
			engine.RegisterCommand("print", () => new PrintCommand());
			engine.RegisterCommand("bypass", () => new BypassCommand());
			engine.RegisterCommand("elements", () => new ElementsCommand());

			var compileResult = engine.CompileSource(source, Mode.Debug, null);
			if (compileResult.errors.count > 0)
			{
				var formattedErrors = GetFormattedCompileErrors(compileResult.errors, source);
				System.Console.WriteLine(formattedErrors);
			}
			else
			{
				var sb = new StringBuilder();
				compileResult.Disassemble(sb);
				System.Console.WriteLine(sb);

				var executeResult = engine.Execute(compileResult);
				if (executeResult.isSome)
					System.Console.WriteLine(executeResult.value.message.Format());

				System.Console.WriteLine("FINISH");
			}
		}
	}
}
