using System.Text;

namespace Flow
{
	public readonly struct CompileResult
	{
		public readonly Buffer<CompileError> errors;
		internal readonly ByteCodeChunk chunk;
		internal readonly Buffer<Source> sources;

		internal CompileResult(Buffer<CompileError> errors, ByteCodeChunk chunk, Buffer<Source> sources)
		{
			this.errors = errors;
			this.chunk = chunk;
			this.sources = sources;
		}

		public void Disassemble(StringBuilder sb)
		{
			chunk.Disassemble(sb);
		}
	}
}