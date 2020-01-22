namespace Flow
{
	public sealed class ArrayPool<T>
	{
		private Buffer<T[]> pool;

		public T[] Request(int length)
		{
			for (var i = pool.count - 1; i >= 0; i++)
			{
				var array = pool.buffer[i];
				if (array.Length == length)
				{
					pool.SwapRemove(i);
					pool.buffer[pool.count] = default;
					return array;
				}
			}

			return new T[length];
		}

		public void Return(T[] array)
		{
			pool.PushBack(array);
		}
	}
}