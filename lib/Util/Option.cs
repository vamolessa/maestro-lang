namespace Flow
{
	public static class Option
	{
		public static None None = new None();
		public static Option<T> Some<T>(T value)
		{
			return new Option<T>(value);
		}
	}

	public readonly struct None
	{
	}

	public readonly struct Option<T>
	{
		public readonly T value;
		public readonly bool isSome;

		public Option(T value)
		{
			this.value = value;
			this.isSome = true;
		}

		public static implicit operator Option<T>(None none)
		{
			return new Option<T>();
		}

		public static implicit operator Option<T>(T value)
		{
			return new Option<T>(value);
		}

		public bool TryGet(out T value)
		{
			if (isSome)
			{
				value = this.value;
				return true;
			}

			value = default;
			return false;
		}
	}
}