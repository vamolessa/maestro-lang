using System.Text;

namespace Maestro
{
	public readonly struct CompileResult
	{
		public readonly Buffer<CompileError> errors;
		internal readonly Executable<Tuple0> executable;

		internal CompileResult(Buffer<CompileError> errors, Executable<Tuple0> executable)
		{
			this.errors = errors;
			this.executable = executable;
		}

		public bool TryGetExecutable(out Executable<Tuple0> executable)
		{
			if (errors.count == 0)
			{
				executable = this.executable;
				return true;
			}
			else
			{
				executable = default;
				return false;
			}
		}

		public void FormatDisassembledByteCode(StringBuilder sb)
		{
			if (errors.count == 0)
				executable.chunk.Disassemble(sb);
		}

		public void FormatErrors(StringBuilder sb)
		{
			for (var i = 0; i < errors.count; i++)
			{
				var error = errors.buffer[i];
				sb.Append(error.message.Format());

				if (executable.chunk.sources.count == 0)
					continue;

				if (error.slice.index > 0 || error.slice.length > 0)
				{
					var source = executable.chunk.sources.buffer[error.sourceIndex];
					FormattingHelper.AddHighlightSlice(
						source.uri.value,
						source.content,
						error.slice,
						sb
					);
				}
			}
		}
	}

	public readonly struct ExecuteResult
	{
		public readonly Option<RuntimeError> error;
		internal readonly ByteCodeChunk chunk;
		internal readonly Buffer<StackFrame> stackFrames;

		internal ExecuteResult(Option<RuntimeError> error, ByteCodeChunk chunk, Buffer<StackFrame> stackFrames)
		{
			this.error = error;
			this.chunk = chunk;
			this.stackFrames = stackFrames;
		}

		public void FormatError(StringBuilder sb)
		{
			if (!error.isSome)
				return;

			sb.Append(error.value.message.Format());

			if (error.value.instructionIndex < 0 || chunk.sources.count == 0)
				return;

			var source = chunk.sources.buffer[chunk.FindSourceIndex(error.value.instructionIndex)];
			FormattingHelper.AddHighlightSlice(source.uri.value, source.content, error.value.slice, sb);
		}

		public void FormatCallStackTrace(StringBuilder sb)
		{
			if (!error.isSome)
				return;

			for (var i = stackFrames.count - 1; i >= 0; i--)
			{
				var frame = stackFrames.buffer[i];
				var codeIndex = System.Math.Max(frame.codeIndex - 1, 0);
				var sourceIndex = chunk.sourceSlices.buffer[codeIndex].index;

				var commandName = string.Empty;
				if (frame.commandIndex >= 0)
					commandName = chunk.commandDefinitions.buffer[frame.commandIndex].name;

				if (chunk.sources.count == 0)
				{
					sb.AppendLine(commandName);
					continue;
				}

				var source = chunk.sources.buffer[chunk.FindSourceIndex(codeIndex)];

				var pos = FormattingHelper.GetLineAndColumn(
					source.content,
					sourceIndex
				);
				sb.Append("[line ");
				sb.Append(pos.lineIndex + 1);
				sb.Append("] ");
				sb.Append(commandName);
				sb.Append(": ");

				var slice = FormattingHelper.GetLineSlice(source.content, pos.lineIndex);
				slice = FormattingHelper.Trim(source.content, slice);
				sb.Append(source.content, slice.index, slice.length);
				sb.AppendLine();
			}
		}
	}
}