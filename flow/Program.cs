using System.Text;

namespace Flow
{
	public static class Program
	{
		public sealed class PrintCommand : ICommand<Tuple0, Tuple0>
		{
			public Result<Tuple0> Execute(Inputs inputs, Tuple0 args)
			{
				for (var i = 0; i < inputs.count; i++)
					System.Console.WriteLine(inputs[i].ToString());

				return default;
			}
		}

		public sealed class BypassCommand : ICommand<Tuple0, Tuple1>
		{
			public Result<Tuple1> Execute(Inputs inputs, Tuple0 args)
			{
				return inputs.count > 0 ? inputs[0] : new Value(null);
			}
		}

		public sealed class ElementsCommand : ICommand<Tuple0, Tuple1>
		{
			public int currentIndex = 0;

			public Result<Tuple1> Execute(Inputs inputs, Tuple0 args)
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

			var sb = new StringBuilder();

			var compileResult = engine.CompileSource(source, Mode.Debug, null);
			if (compileResult.HasErrors)
			{
				sb.Clear();
				compileResult.FormatErrors(sb);
				System.Console.WriteLine(sb);
			}
			else
			{
				sb.Clear();
				compileResult.FormatDisassembledByteCode(sb);
				System.Console.WriteLine(sb);

				var executeResult = engine.Execute(compileResult);
				if (executeResult.HasError)
				{
					sb.Clear();
					executeResult.FormatError(sb);
					executeResult.FomratCallStackTrace(sb);
					System.Console.WriteLine(sb);
				}

				System.Console.WriteLine("FINISH");
			}
		}
	}
}
