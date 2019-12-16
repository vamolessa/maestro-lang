namespace Rain
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var content = System.IO.File.ReadAllText("scripts/script.rain");
			var compiler = new Compiler();
			var errors = compiler.CompileSource(new Source(new Uri("script.rain"), content));

			System.Console.WriteLine("ERRROR COUNT: {0}", errors.count);
			for (var i = 0; i < errors.count; i++)
			{
				var error = errors.buffer[i];
				System.Console.WriteLine("{0} {1}", error.type, error.context);
			}
		}
	}
}
