namespace Maestro
{
	public readonly struct Executable<T> where T : struct, ITuple
	{
		internal readonly ByteCodeChunk chunk;
		internal readonly ExternCommandCallback[] externCommandInstances;
		public readonly int commandIndex;

		internal Executable(ByteCodeChunk chunk, ExternCommandCallback[] externCommandInstances, int commandIndex)
		{
			this.chunk = chunk;
			this.externCommandInstances = externCommandInstances;
			this.commandIndex = commandIndex;
		}
	}
}