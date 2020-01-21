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

		public sealed class MyCommand : ICommand
		{
			public readonly string name;

			public MyCommand(string name)
			{
				this.name = name;
			}

			public object Invoke(object input, object[] args)
			{
				var inputText = input != null ? input.ToString() : "null";
				System.Console.WriteLine($"THIS IS A HELLO FROM MY COMMAND {name} WITH INPUT {inputText}");
				return name;
			}

			public static Command New(string name, byte paramCount)
			{
				return new Command(name, paramCount, () => new MyCommand(name));
			}
		}

		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.flow");
			var source = new Source(new Uri("script.flow"), content);

			var chunk = new ByteCodeChunk();
			chunk.RegisterCommand(MyCommand.New("command", 1));
			chunk.RegisterCommand(MyCommand.New("print", 0));

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
