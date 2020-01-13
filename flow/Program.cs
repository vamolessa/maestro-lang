namespace Flow
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.flow");
			var io = new TokenizerIO();
			io.source = content;
			while (!io.IsAtEnd())
			{
				var c = io.NextChar();
				switch (c)
				{
				case '\t':
					System.Console.WriteLine("INDENT");
					break;
				case '\b':
					System.Console.WriteLine("DEDENT");
					break;
				case '\n':
					System.Console.WriteLine(" NEWLINE");
					break;
				default:
					System.Console.Write(c);
					break;
				}
			}
			System.Console.WriteLine();
			System.Console.WriteLine("END");
			return;

			var compiler = new Compiler();
			var errors = compiler.CompileSource(new Source(new Uri("script.flow"), content));

			System.Console.WriteLine("ERRROR COUNT: {0}", errors.count);
			for (var i = 0; i < errors.count; i++)
			{
				var error = errors.buffer[i];
				System.Console.WriteLine("{0} {1}", error.type, error.context);
			}
		}
	}
}
