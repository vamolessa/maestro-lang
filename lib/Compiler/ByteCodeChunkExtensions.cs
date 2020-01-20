using System.Text;

namespace Flow
{
	public sealed class ByteCodeChunkDebugView
	{
		public readonly string[] lines;

		public ByteCodeChunkDebugView(ByteCodeChunk chunk)
		{
			var sb = new StringBuilder();
			chunk.Disassemble(sb);
			lines = sb.ToString().Split(new string[] { "\n", "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
		}
	}

	public static class ByteCodeChunkExtensions
	{
		public static int FindSourceIndex(this ByteCodeChunk self, int codeIndex)
		{
			for (var i = 0; i < self.sourceStartIndexes.count; i++)
			{
				if (codeIndex >= self.sourceStartIndexes.buffer[i])
					return i;
			}

			return -1;
		}

		public static void Disassemble(this ByteCodeChunk self, StringBuilder sb)
		{
			sb.Append("== ");
			sb.Append(self.bytes.count);
			sb.AppendLine(" bytes ==");
			sb.AppendLine("byte instruction");

			for (var index = 0; index < self.bytes.count;)
			{
				index = DisassembleInstruction(self, index, sb);
				sb.AppendLine();
			}
			sb.AppendLine("== end ==");
		}

		internal static void Disassemble(this ByteCodeChunk self, Source[] sources, StringBuilder sb)
		{
			var currentSourceIndex = -1;

			sb.Append("== ");
			sb.Append(self.bytes.count);
			sb.AppendLine(" bytes ==");
			sb.AppendLine("line byte instruction");

			for (var index = 0; index < self.bytes.count;)
			{
				var sourceIndex = self.FindSourceIndex(index);
				var source = sources[sourceIndex];
				if (sourceIndex != currentSourceIndex)
				{
					sb.Append("== ");
					sb.Append(source.uri);
					sb.AppendLine(" ==");
					currentSourceIndex = sourceIndex;
				}

				PrintLineNumber(self, source.content, index, sb);
				index = DisassembleInstruction(self, index, sb);
				sb.AppendLine();
			}

			sb.AppendLine("== end ==");
		}

		private static void PrintLineNumber(ByteCodeChunk self, string source, int index, StringBuilder sb)
		{
			var currentSourceIndex = self.sourceSlices.buffer[index].index;
			var currentPosition = FormattingHelper.GetLineAndColumn(source, currentSourceIndex);
			var lastLineIndex = -1;
			if (index > 0)
			{
				var lastSourceIndex = self.sourceSlices.buffer[index - 1].index;
				lastLineIndex = FormattingHelper.GetLineAndColumn(source, lastSourceIndex).lineIndex;
			}

			if (currentPosition.lineIndex == lastLineIndex)
				sb.Append("   | ");
			else
				sb.AppendFormat("{0,4} ", currentPosition.lineIndex);
		}

		public static int DisassembleInstruction(this ByteCodeChunk self, int index, StringBuilder sb)
		{
			sb.AppendFormat("{0:0000} ", index);

			var instructionCode = self.bytes.buffer[index];
			var instruction = (Instruction)instructionCode;

			switch (instruction)
			{
			case Instruction.Halt:
			case Instruction.ClearStack:
			case Instruction.Pop:
			case Instruction.LoadNull:
			case Instruction.LoadFalse:
			case Instruction.LoadTrue:
				return OneByteInstruction(instruction, index, sb);
			case Instruction.CreateArray:
			case Instruction.AssignLocal:
			case Instruction.LoadLocal:
				return TwoByteInstruction(self, instruction, index, sb);
			case Instruction.LoadLiteral:
				return LoadLiteralInstruction(self, instruction, index, sb);
			case Instruction.AddLocalName:
				return AddLocalNameInstruction(self, instruction, index, sb);
			case Instruction.CallNativeCommand:
				return CallCommandInstruction(self, instruction, index, sb);
			default:
				sb.Append("Unknown instruction ");
				sb.Append(instruction.ToString());
				return index + 1;
			}
		}

		private static int OneByteInstruction(Instruction instruction, int index, StringBuilder sb)
		{
			sb.Append(instruction.ToString());
			return index + 1;
		}

		private static int TwoByteInstruction(ByteCodeChunk chunk, Instruction instruction, int index, StringBuilder sb)
		{
			sb.Append(instruction.ToString());
			sb.Append(' ');
			sb.Append(chunk.bytes.buffer[index + 1]);
			return index + 2;
		}

		private static int LoadLiteralInstruction(ByteCodeChunk chunk, Instruction instruction, int index, StringBuilder sb)
		{
			var literalIndex = BytesHelper.BytesToUShort(
				chunk.bytes.buffer[index + 1],
				chunk.bytes.buffer[index + 2]
			);
			var value = chunk.literals.buffer[literalIndex];

			sb.Append(instruction.ToString());
			sb.Append(value);
			sb.Append(" #");
			sb.Append(value.GetType().Name);

			return index + 3;
		}

		private static int AddLocalNameInstruction(ByteCodeChunk chunk, Instruction instruction, int index, StringBuilder sb)
		{
			var literalIndex = BytesHelper.BytesToUShort(
				chunk.bytes.buffer[index + 1],
				chunk.bytes.buffer[index + 2]
			);
			var name = chunk.literals.buffer[literalIndex];

			sb.Append(instruction.ToString());
			sb.Append(' ');
			sb.Append(name);

			return index + 3;
		}

		private static int CallCommandInstruction(ByteCodeChunk chunk, Instruction instruction, int index, StringBuilder sb)
		{
			var instanceIndex = BytesHelper.BytesToUShort(
				chunk.bytes.buffer[index + 1],
				chunk.bytes.buffer[index + 2]
			);

			var commandIndex = chunk.commandInstances.buffer[instanceIndex];
			var command = chunk.commands.buffer[commandIndex];

			sb.Append(instruction.ToString());
			sb.Append(' ');
			sb.Append(command.name);

			return index + 3;
		}
	}
}