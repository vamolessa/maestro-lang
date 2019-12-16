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
			io.BeginSource(source.content, compiledSources.count);
			compiledSources.PushBack(source);

			//var finishedModuleImports = false;
			io.parser.Next();
			while (!io.parser.Match(TokenKind.End))
				SubGraph();

			io.EndSource();
		}

		private TransformNode SubGraph()
		{
			return SubGraph(null, 0);
		}

		private TransformNode SubGraph(TransformNode source, int indentation)
		{
			var node = Node(indentation);
			if (source != null)
				node.source.PushBack(source);

			while (!io.parser.Match(TokenKind.NewLine) && !io.parser.Match(TokenKind.End))
			{
				var newIndentation = Indentation();

				if (newIndentation < indentation)
				{
					break;
				}
				else if (newIndentation > indentation)
				{
					var subIndentation = newIndentation;
					do
					{
						var newNode = SubGraph(node, newIndentation);
						newIndentation = Indentation();
					} while (newIndentation == subIndentation);
				}
				else
				{
					var newNode = Node(newIndentation);
					newNode.source.PushBack(node);
					node = newNode;
				}
			}

			while (io.parser.Match(TokenKind.NewLine) || io.parser.Match(TokenKind.Tab))
				continue;

			return node;
		}

		private int Indentation()
		{
			var indentation = 0;
			while (io.parser.Match(TokenKind.Tab))
				indentation += 1;

			return indentation;
		}

		private TransformNode Node(int indentation)
		{
			io.parser.Consume(TokenKind.Identifier, CompileErrorType.ExpectedNodeName);
			var slice = io.parser.previousToken.slice;
			var node = new TransformNode(slice);

			io.parser.Consume(TokenKind.NewLine, CompileErrorType.ExpectedNewLine);

			System.Console.WriteLine("NODE: '{0}' INDENT: {1}", io.parser.tokenizer.source.Substring(slice.index, slice.length), indentation);

			return node;
		}
	}
}