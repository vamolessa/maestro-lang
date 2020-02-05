namespace Maestro
{
	public readonly struct Executable
	{
		internal readonly ByteCodeChunk chunk;
		internal readonly ExternalCommandCallback[] externalCommandInstances;
		internal readonly Source[] sources;

		internal Executable(ByteCodeChunk chunk, ExternalCommandCallback[] externalCommandInstances, Source[] sources)
		{
			this.chunk = chunk;
			this.externalCommandInstances = externalCommandInstances;
			this.sources = sources;
		}
	}
}