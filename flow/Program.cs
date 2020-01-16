using System.Text;

namespace Flow
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.flow");

			var chunk = new ByteCodeChunk();
			chunk.RegisterCommand(new Command("command", null));
			chunk.RegisterCommand(new Command("print", null));

			var compiler = new Compiler();
			var errors = compiler.CompileSource(chunk, new Source(new Uri("script.flow"), content));

			if (errors.count > 0)
			{
				for (var i = 0; i < errors.count; i++)
				{
					var error = errors.buffer[i];
					System.Console.WriteLine(error.message.Format());
				}
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
