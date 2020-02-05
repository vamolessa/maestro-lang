using System.Text;

namespace Maestro
{
	public struct StackFrame
	{
		public int codeIndex;
		public int stackIndex;
		public int commandIndex;

		public StackFrame(int codeIndex, int stackIndex, int commandIndex)
		{
			this.codeIndex = codeIndex;
			this.stackIndex = stackIndex;
			this.commandIndex = commandIndex;
		}
	}

	public struct DebugInfo
	{
		public readonly struct DebugFrame
		{
			public readonly int variableInfosIndex;

			public DebugFrame(int variableInfosIndex)
			{
				this.variableInfosIndex = variableInfosIndex;
			}
		}

		public readonly struct VariableInfo
		{
			public readonly string name;
			public readonly int stackIndex;

			public VariableInfo(string name, int stackIndex)
			{
				this.name = name;
				this.stackIndex = stackIndex;
			}
		}

		public Buffer<DebugFrame> frames;
		public Buffer<VariableInfo> variableInfos;

		public void Clear()
		{
			variableInfos.ZeroClear();
		}

		public void PushFrame()
		{
			frames.PushBack(new DebugFrame(
				variableInfos.count
			));
		}

		public void PopFrame()
		{
			var frame = frames.PopLast();
			variableInfos.count = frame.variableInfosIndex;
		}
	}

	public sealed class VirtualMachine
	{
		public Buffer<StackFrame> stackFrames = new Buffer<StackFrame>(4);
		public Buffer<Value> stack = new Buffer<Value>(32);
		internal Buffer<int> tupleSizes = new Buffer<int>(4);
		internal Buffer<Slice> inputSlices = new Buffer<Slice>(4);

		public DebugInfo debugInfo;
		internal Option<IDebugger> debugger;

		public RuntimeError NewError(ByteCodeChunk chunk, IFormattedMessage message)
		{
			var ip = -1;
			if (stackFrames.count > 0)
				ip = stackFrames.buffer[stackFrames.count - 1].codeIndex;

			return new RuntimeError(
				ip,
				ip >= 0 ? chunk.sourceSlices.buffer[ip] : new Slice(),
				message
			);
		}

		internal Option<int> FindVariableIndex(int stackIndex)
		{
			for (var i = 0; i < debugInfo.variableInfos.count; i++)
			{
				var variableInfo = debugInfo.variableInfos.buffer[i];
				if (variableInfo.stackIndex == stackIndex)
					return i;
			}

			return Option.None;
		}

		internal void TraceStack(StringBuilder sb)
		{
			sb.Append("     ");
			for (var i = 0; i < stack.count; i++)
			{
				sb.Append('(');

				var variableIndex = FindVariableIndex(i);
				if (variableIndex.isSome)
				{
					var variableInfo = debugInfo.variableInfos.buffer[variableIndex.value];
					sb.Append(variableInfo.name);
					sb.Append(": ");
				}

				stack.buffer[i].AppendTo(sb);
				sb.Append(") ");
			}

			if (stack.count == 0)
				sb.Append("-");

			sb.AppendLine();
		}
	}
}