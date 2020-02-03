using System.Text;

namespace Maestro
{
	internal static class VirtualMachineHelper
	{
		internal static Option<int> FindVariableIndex(VirtualMachine vm, int stackIndex)
		{
			for (var i = 0; i < vm.debugInfo.variableInfos.count; i++)
			{
				var variableInfo = vm.debugInfo.variableInfos.buffer[i];
				if (variableInfo.stackIndex == stackIndex)
					return i;
			}

			return Option.None;
		}

		internal static void TraceStack(VirtualMachine vm, StringBuilder sb)
		{
			sb.Append("     ");
			for (var i = 0; i < vm.stack.count; i++)
			{
				sb.Append('(');

				var variableIndex = FindVariableIndex(vm, i);
				if (variableIndex.isSome)
				{
					var variableInfo = vm.debugInfo.variableInfos.buffer[variableIndex.value];
					sb.Append(variableInfo.name);
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