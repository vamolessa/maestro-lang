namespace Rain
{
	public interface IParseNode
	{
	}

	public sealed class Variable : IParseNode
	{
		public readonly Slice slice;

		public Variable(Slice slice)
		{
			this.slice = slice;
		}
	}

	public sealed class TransformNode : IParseNode
	{
		public readonly Slice slice;
		public Buffer<IParseNode> source;

		public TransformNode(Slice slice)
		{
			this.slice = slice;
			this.source = new Buffer<IParseNode>();
		}
	}
}