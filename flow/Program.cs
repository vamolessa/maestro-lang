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
			public Tuple0 Invoke(VirtualMachine vm, int inputCount, Tuple0 args)
			{
				System.Console.WriteLine($"PRINTING {inputCount} INPUTS:");
				for (var i = 0; i < inputCount; i++)
					System.Console.WriteLine(vm.GetInput(i).ToString());

				return default;
			}
		}

		public sealed class BypassCommand : ICommand<Tuple0, Tuple1>
		{
			public Tuple1 Invoke(VirtualMachine vm, int inputCount, Tuple0 args)
			{
				System.Console.WriteLine($"BYPASS WITH {inputCount} INPUTS");
				return inputCount > 0 ? vm.GetInput(0) : new Value(null);
			}
		}

		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.flow");
			var source = new Source(new Uri("script.flow"), content);

			var chunk = new ByteCodeChunk();
			chunk.RegisterCommand(CommandDefinition.Create("print", () => new PrintCommand()));
			chunk.RegisterCommand(CommandDefinition.Create("bypass", () => new BypassCommand()));

			var controller = new CompilerController();
			var errors = controller.CompileSource(chunk, source);

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

				var vm = new VirtualMachine();
				vm.Load(chunk);

				vm.callFrameStack.PushBackUnchecked(new CallFrame(0, 0, 0, CallFrame.Type.EntryPoint));
				VirtualMachineInstructions.Run(vm);

				System.Console.WriteLine("FINISH");
			}
		}
	}
}
