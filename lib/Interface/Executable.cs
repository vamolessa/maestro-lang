namespace Maestro
{
	public readonly struct Executable<T> where T : struct, ITuple
	{
		internal readonly ByteCodeChunk chunk;
		internal readonly ExternalCommandCallback[] externalCommandInstances;
		internal readonly Source[] sources;
		public readonly int commandIndex;

		internal Executable(ByteCodeChunk chunk, ExternalCommandCallback[] externalCommandInstances, Source[] sources, int commandIndex)
		{
			this.chunk = chunk;
			this.externalCommandInstances = externalCommandInstances;
			this.sources = sources;
			this.commandIndex = commandIndex;
		}
	}
}