using System.Diagnostics;

namespace Maestro
{
	[DebuggerTypeProxy(typeof(BufferDebugView<>))]
	public struct Buffer<T>
	{
		public const int MinCapacity = 4;

		public int count;
		public T[] buffer;

		public Buffer(int capacity)
		{
			count = 0;
			buffer = new T[capacity >= MinCapacity ? capacity : MinCapacity];
		}

		public void ZeroClear()
		{
			count = 0;
			if (buffer != null)
				System.Array.Clear(buffer, 0, buffer.Length);
		}

		public void Grow(int size)
		{
			if (buffer == null)
				buffer = new T[MinCapacity];
			GrowUnchecked(size);
		}

		public void GrowUnchecked(int size)
		{
			count += size;

			if (count > buffer.Length)
			{
				var previousCount = count - size;
				var newLength = buffer.Length << 1;
				while (newLength < count)
					newLength <<= 1;
				var temp = new T[newLength];
				System.Array.Copy(buffer, temp, previousCount);
				buffer = temp;
			}
		}

		public void PushBack(T element)
		{
			if (buffer == null)
				buffer = new T[MinCapacity];
			PushBackUnchecked(element);
		}

		public void PushBackUnchecked(T element)
		{
			if (count >= buffer.Length)
			{
				var temp = new T[buffer.Length << 1];
				System.Array.Copy(buffer, temp, buffer.Length);
				buffer = temp;
			}

			buffer[count++] = element;
		}

		public T PopLast()
		{
			return buffer[--count];
		}

		public void SwapRemove(int index)
		{
			buffer[index] = buffer[--count];
		}

		public T[] ToArray()
		{
			if (buffer == null || count == 0)
				return new T[0];

			var array = new T[count];
			System.Array.Copy(buffer, 0, array, 0, array.Length);
			return array;
		}
	}

	public sealed class BufferDebugView<T>
	{
		public readonly T[] elements;

		public BufferDebugView(Buffer<T> buffer)
		{
			if (buffer.buffer != null)
			{
				elements = new T[buffer.count];
				System.Array.Copy(buffer.buffer, elements, buffer.count);
			}
			else
			{
				elements = new T[0];
			}
		}
	}
}