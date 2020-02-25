using System.Text;
using Maestro.StdLib;

namespace Maestro
{
	public static class Program
	{
		public sealed class PrintCommand : ICommand<Tuple0>
		{
			private static int instanceCount = 0;
			private int instanceNumber;

			public PrintCommand()
			{
				instanceNumber = instanceCount++;
			}

			public void Execute(ref Context context, Tuple0 args)
			{
				var sb = new StringBuilder();
				for (var i = 0; i < context.inputCount; i++)
				{
					context.GetInput(i).AppendTo(sb);
					sb.Append(' ');
				}
				System.Console.WriteLine(sb);
			}
		}

		public sealed class BypassCommand : ICommand<Tuple0>
		{
			public void Execute(ref Context context, Tuple0 args)
			{
				for (var i = 0; i < context.inputCount; i++)
					context.PushValue(context.GetInput(i));
			}
		}

		public static void Main(string[] args)
		{
			/*
			var debugger = new Debug.Debugger();
			debugger.Start(47474);
			System.Console.WriteLine("WAITING FOR CLIENT");
			debugger.WaitForClient();
			Run(debugger);
			debugger.Stop();
			/*/
			Run(Option.None);
			//*/
		}

		public static void Run(Option<IDebugger> debugger)
		{
			var content = System.IO.File.ReadAllText("Scripts/Script.maestro");
			var source = new Source("Script.maestro", content);

			var engine = new Engine();
			engine.SetDebugger(debugger);
			engine.RegisterStandardCommands(t => System.Console.WriteLine(t));

			var sb = new StringBuilder();
			var compileResult = engine.CompileSource(source, Mode.Debug);
			if (compileResult.TryGetExecutable(out var executable))
			{
				sb.Clear();
				compileResult.FormatDisassembledByteCode(sb);
				System.Console.WriteLine(sb);

				System.Console.WriteLine("RUN\n");
				var executeResult = engine.ExecuteScope().Execute(executable);
				if (executeResult.error.isSome)
				{
					sb.Clear();
					executeResult.FormatError(sb);
					executeResult.FormatCallStackTrace(sb);
					System.Console.WriteLine(sb);
				}

				System.Console.WriteLine("FINISH");
			}
			else
			{
				sb.Clear();
				compileResult.FormatErrors(sb);
				System.Console.WriteLine(sb);
			}
		}
	}
}
