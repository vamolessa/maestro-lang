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
		internal sealed class Data
		{
			public readonly RuntimeError error;
			internal readonly ByteCodeChunk chunk;
			internal readonly Source[] sources;
			internal readonly Buffer<StackFrame> stackFrames;

			internal Data(RuntimeError error, ByteCodeChunk chunk, Source[] sources, Buffer<StackFrame> stackFrames)
			{
				this.error = error;
				this.chunk = chunk;
				this.sources = sources;
				this.stackFrames = stackFrames;
			}
		}

		internal readonly Data data;

		public bool HasError
		{
			get { return data != null; }
		}

		internal ExecuteResult(Data data)
		{
			this.data = data;
		}

		public void FormatError(StringBuilder sb)
		{
			if (data == null)
				return;

			sb.Append(data.error.message.Format());

			if (data.error.instructionIndex < 0)
				return;

			var source = data.sources[data.chunk.FindSourceIndex(data.error.instructionIndex)];
			FormattingHelper.AddHighlightSlice(source.uri.value, source.content, data.error.slice, sb);
		}

		public void FormatCallStackTrace(StringBuilder sb)
		{
			if (data == null)
				return;

			for (var i = data.stackFrames.count - 1; i >= 0; i--)
			{
				var frame = data.stackFrames.buffer[i];
				var codeIndex = System.Math.Max(frame.codeIndex - 1, 0);
				var sourceIndex = data.chunk.sourceSlices.buffer[codeIndex].index;
				var source = data.sources[data.chunk.FindSourceIndex(codeIndex)];

				var pos = FormattingHelper.GetLineAndColumn(
					source.content,
					sourceIndex
				);
				sb.Append("[line ");
				sb.Append(pos.lineIndex + 1);
				sb.Append("] ");

				if (frame.commandInstanceIndex >= 0)
				{
					var commandName = data.chunk.externalCommandDefinitions.buffer[frame.commandInstanceIndex].name;
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