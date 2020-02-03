using System.Text;

namespace Maestro
{
	internal static class VirtualMachineHelper
	{
		internal static Option<int> FindVariableIndex(VirtualMachine vm, int stackIndex)
		{
			for (var i = 0; i < vm.debugInfo.localVariables.count; i++)
			{
				var localVariable = vm.debugInfo.localVariables.buffer[i];
				if (localVariable.stackIndex == stackIndex)
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
					var localInfo = vm.debugInfo.localVariables.buffer[variableIndex.value];
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