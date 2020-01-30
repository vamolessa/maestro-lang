using System.Text;

namespace Flow
{
	public readonly struct CompileResult
	{
		internal sealed class Data
		{
			internal readonly Buffer<CompileError> errors;
			internal readonly Buffer<Source> sources;

			internal Data(Buffer<CompileError> errors, Buffer<Source> sources)
			{
				this.errors = errors;
				this.sources = sources;
			}
		}

		internal readonly ByteCodeChunk chunk;
		internal readonly Data data;

		public bool HasErrors
		{
			get { return data != null; }
		}

		internal CompileResult(ByteCodeChunk chunk, Data data)
		{
			this.chunk = chunk;
			this.data = data;
		}

		public void FormatDisassembledByteCode(StringBuilder sb)
		{
			chunk.Disassemble(sb);
		}

		public void FormatErrors(StringBuilder sb)
		{
			if (data == null)
				return;

			for (var i = 0; i < data.errors.count; i++)
			{
				var error = data.errors.buffer[i];
				sb.Append(error.message.Format());

				if (error.slice.index > 0 || error.slice.length > 0)
				{
					var source = data.sources.buffer[error.sourceIndex];
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
			internal readonly Buffer<Source> sources;
			internal readonly Buffer<StackFrame> stackFrames;

			internal Data(RuntimeError error, ByteCodeChunk chunk, Buffer<Source> sources, Buffer<StackFrame> stackFrames)
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

			var source = data.sources.buffer[data.chunk.FindSourceIndex(data.error.instructionIndex)];
			FormattingHelper.AddHighlightSlice(source.uri.value, source.content, data.error.slice, sb);
		}

		public void FomratCallStackTrace(StringBuilder sb)
		{
			if (data == null)
				return;

			for (var i = data.stackFrames.count - 1; i >= 0; i--)
			{
				var frame = data.stackFrames.buffer[i];
				var codeIndex = System.Math.Max(frame.callingCodeIndex - 1, 0);
				var sourceIndex = data.chunk.sourceSlices.buffer[codeIndex].index;
				var source = data.sources.buffer[data.chunk.FindSourceIndex(codeIndex)];

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