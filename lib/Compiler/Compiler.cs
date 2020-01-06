[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("rain")]

namespace Rain
{
	internal sealed class Compiler
	{
		public readonly CompilerIO io = new CompilerIO();
		public Buffer<Source> compiledSources = new Buffer<Source>(1);

		private int indentation;

		public Buffer<CompileError> CompileSource(Source source)
		{
			compiledSources.count = 0;

			System.Console.WriteLine("==== TOKENS ====");
			io.parser.tokenizer.Reset(source.content, 0);
			while (true)
			{
				var token = io.parser.tokenizer.Next();
				switch (token.kind)
				{
				case TokenKind.NewLine:
				case TokenKind.Indent:
				case TokenKind.Dedent:
				case TokenKind.End:
					System.Console.WriteLine("{0}", token.kind);
					break;
				default:
					System.Console.WriteLine("{0}: '{1}'", token.kind, source.content.Substring(token.slice.index, token.slice.length));
					break;
				}
				if (token.kind == TokenKind.End)
					break;
			}
			System.Console.WriteLine("==== END ====");
			System.Console.WriteLine();
			return new Buffer<CompileError>();

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
			while (!io.parser.Match(TokenKind.End))
				SubGraph(0);

			io.EndSource();
		}

		private void ConsumeIndentation()
		{
			indentation = 0;
			while (io.parser.Match(TokenKind.Indent))
				indentation += 1;
		}

		private void SubGraph(int argCount)
		{
			System.Console.WriteLine("SUBGRAPH WITH {0} ARGS", argCount);
			Node(argCount);
			argCount = 1;

			var graphIndentation = indentation;

			while (!io.parser.Match(TokenKind.NewLine) && !io.parser.Match(TokenKind.End))
			{
				ConsumeIndentation();

				if (indentation < graphIndentation)
				{
					if (indentation < graphIndentation - 1)
						System.Console.WriteLine("POP {0} UNUSED VALUES", argCount);
					return;
				}
				else if (indentation > graphIndentation)
				{
					argCount = 0;
					var subIndentation = indentation;
					do
					{
						System.Console.WriteLine("COPY VALUE AT OFFSET -{0}", argCount + 1);
						SubGraph(1);
						ConsumeIndentation();
						argCount += 1;
					} while (subIndentation == indentation);

					System.Console.WriteLine("DELETE VALUE AT OFFSET -{0}", argCount + 1);
				}
				else
				{
					Node(argCount);
					argCount = 1;
				}
			}

			if (indentation == 0)
				System.Console.WriteLine("POP {0} VALUES AT INDENT 0", argCount);
		}

		private void Node(int argCount)
		{
			io.parser.Consume(TokenKind.Identifier, CompileErrorType.ExpectedNodeName);
			var slice = io.parser.previousToken.slice;

			io.parser.Consume(TokenKind.NewLine, CompileErrorType.ExpectedNewLine);

			System.Console.WriteLine("NODE: '{0}' WITH {1} ARGS", io.parser.tokenizer.io.source.Substring(slice.index, slice.length), argCount);
		}
	}
}