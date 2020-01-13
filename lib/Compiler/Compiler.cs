[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("flow")]

namespace Flow
{
	internal sealed class Compiler
	{
		public readonly CompilerIO io = new CompilerIO();
		public Buffer<Source> compiledSources = new Buffer<Source>(1);

		private int indentation;

		public Buffer<CompileError> CompileSource(Source source)
		{
			compiledSources.count = 0;

			io.Reset();
			Compile(source);
			return io.errors;
		}

		private void Compile(Source source)
		{
			io.BeginSource(source.content, compiledSources.count);
			compiledSources.PushBack(source);

			//var finishedModuleImports = false;
			indentation = 0;
			io.parser.Next();

			io.EndSource();
		}
	}
}