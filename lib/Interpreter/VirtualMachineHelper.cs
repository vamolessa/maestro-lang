using System.Text;

namespace Flow
{
	public static class VirtualMachineHelper
	{
		public static Value[] CreateArray(this VirtualMachine self, int length)
		{
			return self.arrayPool.Request(length);
		}

		internal static Value DeepCopy(VirtualMachine vm, Value value)
		{
			if (value.kind == ValueKind.Array)
			{
				var array = value.asObject as Value[];
				var copy = vm.CreateArray(array.Length);
				for (var i = 0; i < array.Length; i++)
					copy[i] = DeepCopy(vm, array[i]);

				return new Value(copy);
			}
			else
			{
				return value;
			}
		}

		internal static void Collect(VirtualMachine vm, ref Value value)
		{
			if (value.kind == ValueKind.Array)
			{
				var array = value.asObject as Value[];
				vm.arrayPool.Return(array);

				for (var i = 0; i < array.Length; i++)
					Collect(vm, ref array[i]);
			}

			value = default;
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