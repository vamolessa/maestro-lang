﻿using System.Text;

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
				System.Console.WriteLine("PRINT INSTANCE NUMBER {0}", instanceNumber);

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

		public sealed class TestCommand : ICommand<Tuple2>
		{
			public void Execute(ref Context context, Tuple2 args)
			{
				// for (var i = 0; i < context.inputCount; i++)
				// 	context.PushValue(context.GetInput(0));

				(var arg0, var arg1) = args;
				System.Console.WriteLine("TEST COMMAND WITH {0} INPUTS AND ARGS {1}, {2}", context.inputCount, arg0, arg1);
				context.PushValue(arg0);
				context.PushValue(arg1);
			}
		}

		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.mst");
			var source = new Source(new Uri("script.mst"), content);

			var engine = new Engine();
			engine.RegisterCommand("print", () => new PrintCommand());
			engine.RegisterCommand("bypass", () => new BypassCommand());
			engine.RegisterCommand("test-command", () => new TestCommand());

			var sb = new StringBuilder();

			var compileResult = engine.CompileSource(source, Mode.Debug, null);
			if (compileResult.TryGetExecutable(out var executable))
			{
				sb.Clear();
				compileResult.FormatDisassembledByteCode(sb);
				System.Console.WriteLine(sb);

				var testCommand = engine.InstantiateCommand<Tuple1>(compileResult, "#my-command");
				if (testCommand.isSome)
				{
					System.Console.WriteLine("RUN MY COMMAND\n");
					var executeResult = engine.Execute(testCommand.value, new Value("from C#"));
					if (executeResult.error.isSome)
					{
						sb.Clear();
						executeResult.FormatError(sb);
						executeResult.FormatCallStackTrace(sb);
						System.Console.WriteLine(sb);
					}
				}
				else
				{
					System.Console.WriteLine("RUN ENTIRE BINARY\n");
					var executeResult = engine.Execute(executable, default);
					if (executeResult.error.isSome)
					{
						sb.Clear();
						executeResult.FormatError(sb);
						executeResult.FormatCallStackTrace(sb);
						System.Console.WriteLine(sb);
					}
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
