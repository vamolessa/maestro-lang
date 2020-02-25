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
				executable.assembly.Disassemble(sb);
		}

		public void FormatErrors(StringBuilder sb)
		{
			for (var i = 0; i < errors.count; i++)
			{
				var error = errors.buffer[i];
				sb.Append(error.message.Format());

				if (!executable.assembly.source.HasContent)
					continue;

				if (error.slice.index > 0 || error.slice.length > 0)
				{
					FormattingHelper.AddHighlightSlice(
						executable.assembly.source,
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
		internal readonly Assembly assembly;
		internal readonly Buffer<StackFrame> stackFrames;

		internal ExecuteResult(Option<RuntimeError> error, Assembly assembly, Buffer<StackFrame> stackFrames)
		{
			this.error = error;
			this.assembly = assembly;
			this.stackFrames = stackFrames;
		}

		public void FormatError(StringBuilder sb)
		{
			if (!error.isSome)
				return;

			sb.Append(error.value.message);
			if (error.value.instructionIndex >= 0 && assembly.source.HasContent)
			{
				FormattingHelper.AddHighlightSlice(assembly.source, error.value.slice, sb);
			}
		}

		public void FormatCallStackTrace(StringBuilder sb)
		{
			if (!error.isSome)
				return;

			for (var i = stackFrames.count - 1; i >= 0; i--)
			{
				var frame = stackFrames.buffer[i];
				var codeIndex = System.Math.Max(frame.codeIndex - 1, 0);
				var sourceIndex = assembly.sourceSlices.buffer[codeIndex].index;

				var commandName = string.Empty;
				if (frame.commandIndex >= 0)
					commandName = assembly.commandDefinitions.buffer[frame.commandIndex].name;

				if (!assembly.source.HasContent)
				{
					sb.AppendLine(commandName);
					continue;
				}

				var pos = FormattingHelper.GetLineAndColumn(
					assembly.source.content,
					sourceIndex
				);
				sb.Append("[line ");
				sb.Append(pos.lineIndex + 1);
				sb.Append("] ");
				sb.Append(commandName);
				sb.Append(": ");

				var slice = FormattingHelper.GetLineSlice(assembly.source.content, pos.lineIndex);
				slice = FormattingHelper.Trim(assembly.source.content, slice);
				sb.Append(assembly.source.content, slice.index, slice.length);
				sb.AppendLine();
			}
		}
	}
}