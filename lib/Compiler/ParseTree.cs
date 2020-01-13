namespace Flow
{
	public interface IParseNode
	{
	}

	public sealed class Variable : IParseNode
	{
		public readonly Slice slice;
		public IParseNode source;

		public Variable(Slice slice)
		{
			this.slice = slice;
		}
	}

	public sealed class TransformNode : IParseNode
	{
		public readonly string name;
		public readonly Slice slice;
		public Buffer<IParseNode> source;

		public TransformNode(string name, Slice slice)
		{
			this.name = name;
			this.slice = slice;
			this.source = new Buffer<IParseNode>();
		}
	}
}