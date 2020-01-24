using System.Text;

namespace Flow
{
	public static class VirtualMachineHelper
	{
		public static Value GetInput(this VirtualMachine vm, int index)
		{
			return vm.stack.buffer[index];
		}

		public static void PushValue(this VirtualMachine vm, Value value)
		{
			vm.stack.PushBackUnchecked(value);
		}

		internal static void TraceStack(VirtualMachine vm, StringBuilder sb)
		{
			sb.Append("     ");
			for (var i = 0; i < vm.stack.count; i++)
			{
				sb.Append('(');
				vm.stack.buffer[i].AppendTo(sb);
				sb.Append(") ");
			}

			if (vm.stack.count == 0)
				sb.Append("-");

			sb.AppendLine();
		}
	}
}