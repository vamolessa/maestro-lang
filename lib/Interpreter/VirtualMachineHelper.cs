using System.Text;

namespace Flow
{
	internal static class VirtualMachineHelper
	{
		internal static void TraceStack(VirtualMachine vm, StringBuilder sb)
		{
			sb.Append("     ");
			for (var i = 0; i < vm.stack.count; i++)
			{
				sb.Append('(');
				var localIndex = i - vm.stackFrames.buffer[vm.stackFrames.count - 1].baseStackIndex;
				if (localIndex >= 0 && localIndex < vm.debugInfo.localVariables.count)
				{
					var localInfo = vm.debugInfo.localVariables.buffer[localIndex];
					sb.Append(localInfo.name);
					sb.Append(": ");
				}

				vm.stack.buffer[i].AppendTo(sb);
				sb.Append(") ");
			}

			if (vm.stack.count == 0)
				sb.Append("-");

			sb.AppendLine();
		}
	}
}