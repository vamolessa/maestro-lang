[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("rain")]

namespace Rain
{
	internal sealed class Compiler
	{
		public readonly CompilerIO io = new CompilerIO();
		public Buffer<Source> compiledSources = new Buffer<Source>(1);

		public Buffer<CompileError> CompileSource(Source source)
		{
			compiledSources.count = 0;

			io.Reset();
			Compile(source);
			return io.errors;
		}

		private void Compile(Source source)
		{
			//io.BeginSource(source.content, compiledSources.count);
			compiledSources.PushBack(source);

			//var finishedModuleImports = false;
			io.parser.Next();
			while (!io.parser.Match(TokenKind.End))
				SubGraph();

			//io.EndSource();
		}

		private TransformNode SubGraph()
		{
			(var previousNode, var previousIndentation) = Node();
			var firstNode = previousNode;

			while (!io.parser.Match(TokenKind.NewLine))
			{
				(var node, var indentation) = Node();

				if (indentation < previousIndentation)
				{

				}
				else if (indentation > previousIndentation)
				{
					while (true)
					{
						var n = SubGraph();
						n.source.PushBack(previousNode);
					}
				}
				else
				{
					node.source.PushBack(previousNode);
				}

				previousNode = node;
			}

			while (io.parser.Match(TokenKind.NewLine))
				continue;

			return firstNode;
		}

		private (TransformNode, int) Node()
		{
			var indentation = 0;
			while (io.parser.Match(TokenKind.Tab))
				indentation += 1;

			io.parser.Consume(TokenKind.Identifier, CompileErrorType.ExpectedNodeName);
			io.parser.Consume(TokenKind.NewLine, CompileErrorType.ExpectedNewLine);
			var slice = io.parser.previousToken.slice;
			var node = new TransformNode(slice);

			return (node, indentation);
		}
	}
}