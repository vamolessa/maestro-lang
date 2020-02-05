using System.Text;

namespace Maestro
{
	public readonly struct CompileResult
	{
		public readonly Buffer<CompileError> errors;
		public readonly Option<Executable> executable;

		internal CompileResult(Buffer<CompileError> errors, Option<Executable> executable)
		{
			this.executable = executable;
			this.errors = errors;
		}

		public void FormatDisassembledByteCode(StringBuilder sb)
		{
			if (executable.isSome)
				executable.value.chunk.Disassemble(executable.value
				.sources, sb);
		}

		public void FormatErrors(StringBuilder sb)
		{
			for (var i = 0; i < errors.count; i++)
			{
				var error = errors.buffer[i];
				sb.Append(error.message.Format());

				if (error.slice.index > 0 || error.slice.length > 0)
				{
					var source = executable.value.sources[error.sourceIndex];
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
		internal readonly Executable executable;
		internal readonly Buffer<StackFrame> stackFrames;
		public readonly Option<RuntimeError> error;

		internal ExecuteResult(Executable executable, Buffer<StackFrame> stackFrames, Option<RuntimeError> error)
		{
			this.executable = executable;
			this.stackFrames = stackFrames;
			this.error = error;
		}

		public void FormatError(StringBuilder sb)
		{
			if (!error.isSome)
				return;

			sb.Append(error.value.message.Format());

			if (error.value.instructionIndex < 0)
				return;

			var source = executable.sources[executable.chunk.FindSourceIndex(error.value.instructionIndex)];
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
				var sourceIndex = executable.chunk.sourceSlices.buffer[codeIndex].index;
				var source = executable.sources[executable.chunk.FindSourceIndex(codeIndex)];

				var pos = FormattingHelper.GetLineAndColumn(
					source.content,
					sourceIndex
				);
				sb.Append("[line ");
				sb.Append(pos.lineIndex + 1);
				sb.Append("] ");

				if (frame.commandInstanceIndex >= 0)
				{
					var commandName = executable.chunk.externalCommandDefinitions.buffer[frame.commandInstanceIndex].name;
					sb.Append(commandName);
					sb.Append(": ");
				}

				var slice = FormattingHelper.GetLineSlice(source.content, pos.lineIndex);
				slice = FormattingHelper.Trim(source.content, slice);
				sb.Append(source.content, slice.index, slice.length);
				sb.AppendLine();
			}
		}
	}
}