namespace Maestro
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

	public readonly struct Option<A>
	{
		public readonly A value;
		public readonly bool isSome;

		public Option(A value)
		{
			this.value = value;
			this.isSome = true;
		}

		public static implicit operator Option<A>(None none)
		{
			return new Option<A>();
		}

		public static implicit operator Option<A>(A value)
		{
			return new Option<A>(value);
		}

		public bool TryGet(out A value)
		{
			if (isSome)
			{
				value = this.value;
				return true;
			}

			value = default;
			return false;
		}

		public A GetOr(A defaultValue)
		{
			return isSome ? value : defaultValue;
		}

		public Option<B> Select<B>(System.Func<A, B> function)
		{
			return isSome ?
				new Option<B>(function(value)) :
				default;
		}
	}
}