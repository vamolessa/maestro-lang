namespace Flow
{
	public sealed class TupleSizeIsTooBigException : System.Exception
	{
	}

	public interface ITuple
	{
		byte Size { get; }

		void FromStack(VirtualMachine vm, int index);
		void PushToStack(VirtualMachine vm);
	}

	public struct Tuple0 : ITuple
	{
		public byte Size { get { return 0; } }

		public void FromStack(VirtualMachine vm, int index) { }
		public void PushToStack(VirtualMachine vm) { }
	}

	public struct Tuple1 : ITuple
	{
		public Value value0;

		public byte Size { get { return 1; } }

		public Tuple1(Value value0)
		{
			this.value0 = value0;
		}

		public static implicit operator Tuple1(Value value)
		{
			return new Tuple1(value);
		}

		public static implicit operator Value(Tuple1 self)
		{
			return self.value0;
		}

		public void FromStack(VirtualMachine vm, int index)
		{
			value0 = vm.stack.buffer[index];
		}

		public void PushToStack(VirtualMachine vm)
		{
			vm.stack.PushBackUnchecked(value0);
		}
	}

	public struct Tuple2 : ITuple
	{
		public Value value0;
		public Value value1;

		public byte Size { get { return 2; } }

		public Tuple2(Value value0, Value value1)
		{
			this.value0 = value0;
			this.value1 = value1;
		}

		public static implicit operator Tuple2((Value, Value) tuple)
		{
			return new Tuple2(tuple.Item1, tuple.Item2);
		}

		public void Deconstruct(out Value value0, out Value value1)
		{
			value0 = this.value0;
			value1 = this.value1;
		}

		public void FromStack(VirtualMachine vm, int index)
		{
			value0 = vm.stack.buffer[index];
			value1 = vm.stack.buffer[index + 1];
		}

		public void PushToStack(VirtualMachine vm)
		{
			vm.stack.PushBackUnchecked(value0);
			vm.stack.PushBackUnchecked(value1);
		}
	}

	public struct Tuple3 : ITuple
	{
		public Value value0;
		public Value value1;
		public Value value2;

		public byte Size { get { return 3; } }

		public Tuple3(Value value0, Value value1, Value value2)
		{
			this.value0 = value0;
			this.value1 = value1;
			this.value2 = value2;
		}

		public static implicit operator Tuple3((Value, Value, Value) tuple)
		{
			return new Tuple3(tuple.Item1, tuple.Item2, tuple.Item3);
		}

		public void Deconstruct(out Value value0, out Value value1, out Value value2)
		{
			value0 = this.value0;
			value1 = this.value1;
			value2 = this.value2;
		}

		public void FromStack(VirtualMachine vm, int index)
		{
			value0 = vm.stack.buffer[index];
			value1 = vm.stack.buffer[index + 1];
			value2 = vm.stack.buffer[index + 2];
		}

		public void PushToStack(VirtualMachine vm)
		{
			vm.stack.PushBackUnchecked(value0);
			vm.stack.PushBackUnchecked(value1);
			vm.stack.PushBackUnchecked(value2);
		}
	}

	public struct Tuple4 : ITuple
	{
		public Value value0;
		public Value value1;
		public Value value2;
		public Value value3;

		public byte Size { get { return 4; } }

		public Tuple4(Value value0, Value value1, Value value2, Value value3)
		{
			this.value0 = value0;
			this.value1 = value1;
			this.value2 = value2;
			this.value3 = value3;
		}

		public static implicit operator Tuple4((Value, Value, Value, Value) tuple)
		{
			return new Tuple4(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}

		public void Deconstruct(out Value value0, out Value value1, out Value value2, out Value value3)
		{
			value0 = this.value0;
			value1 = this.value1;
			value2 = this.value2;
			value3 = this.value3;
		}

		public void FromStack(VirtualMachine vm, int index)
		{
			value0 = vm.stack.buffer[index];
			value1 = vm.stack.buffer[index + 1];
			value2 = vm.stack.buffer[index + 2];
			value3 = vm.stack.buffer[index + 3];
		}

		public void PushToStack(VirtualMachine vm)
		{
			vm.stack.PushBackUnchecked(value0);
			vm.stack.PushBackUnchecked(value1);
			vm.stack.PushBackUnchecked(value2);
			vm.stack.PushBackUnchecked(value3);
		}
	}

	public struct Tuple4<T> : ITuple where T : struct, ITuple
	{
		public Value value0;
		public Value value1;
		public Value value2;
		public T value3;

		public byte Size
		{
			get
			{
				var total = 3 + default(T).Size;
				if (total > byte.MaxValue)
					throw new TupleSizeIsTooBigException();
				return (byte)total;
			}
		}

		public Tuple4(Value value0, Value value1, Value value2, T value3)
		{
			this.value0 = value0;
			this.value1 = value1;
			this.value2 = value2;
			this.value3 = value3;
		}

		public static implicit operator Tuple4<T>((Value, Value, Value, T) tuple)
		{
			return new Tuple4<T>(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}

		public void Deconstruct(out Value value0, out Value value1, out Value value2, out T value3)
		{
			value0 = this.value0;
			value1 = this.value1;
			value2 = this.value2;
			value3 = this.value3;
		}

		public void FromStack(VirtualMachine vm, int index)
		{
			value0 = vm.stack.buffer[index];
			value1 = vm.stack.buffer[index + 1];
			value2 = vm.stack.buffer[index + 2];
			value3.FromStack(vm, index + 3);
		}

		public void PushToStack(VirtualMachine vm)
		{
			vm.stack.PushBackUnchecked(value0);
			vm.stack.PushBackUnchecked(value1);
			vm.stack.PushBackUnchecked(value2);
			value3.PushToStack(vm);
		}
	}
}