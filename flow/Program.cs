namespace Flow
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.flow");
			var tokenizer = new Tokenizer(TokenScanners.scanners);
			tokenizer.Reset(content, 0);
			var token = tokenizer.Next();
			while (token.kind != TokenKind.End)
			{
				var slice = tokenizer.source.Substring(token.slice.index, token.slice.length);
				System.Console.WriteLine("{0}: {1}", token.kind, slice);
				token = tokenizer.Next();
			}
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
