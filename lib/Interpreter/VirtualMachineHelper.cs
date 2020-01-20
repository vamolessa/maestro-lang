using System.Text;

namespace Flow
{
	public static class VirtualMachineHelper
	{
		public static void TraceStack(VirtualMachine vm, StringBuilder sb)
		{
			sb.Append("     ");
			for (var i = 0; i < vm.stack.count; i++)
			{
				var value = vm.stack.buffer[i];

				sb.Append('[');
				if (value != null)
				{
					sb.Append(value);
					sb.Append(" #");
					sb.Append(value.GetType().Name);
				}
				else
				{
					sb.Append("null");
				}
				sb.Append(']');
			}

			if (vm.stack.count == 0)
				sb.Append("-");

			sb.AppendLine();
		}
	}
}