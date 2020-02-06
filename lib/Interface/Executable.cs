namespace Maestro
{
	public readonly struct Executable<T> where T : struct, ITuple
	{
		internal readonly ByteCodeChunk chunk;
		internal readonly ExternalCommandCallback[] externalCommandInstances;
		public readonly int commandIndex;

		internal Executable(ByteCodeChunk chunk, ExternalCommandCallback[] externalCommandInstances, int commandIndex)
		{
			this.chunk = chunk;
			this.externalCommandInstances = externalCommandInstances;
			this.commandIndex = commandIndex;
		}
	}
}