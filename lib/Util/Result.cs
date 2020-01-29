namespace Flow
{
	public static class Result
	{
		public static ErrorResult Error(IFormattedMessage errorMessage)
		{
			return new ErrorResult(errorMessage);
		}
	}

	public readonly struct ErrorResult
	{
		public readonly IFormattedMessage errorMessage;

		public ErrorResult(IFormattedMessage errorMessage)
		{
			this.errorMessage = errorMessage;
		}
	}

	public readonly struct Result<T>
	{
		public readonly IFormattedMessage error;
		public readonly T ok;

		public bool IsOk
		{
			get { return error is null; }
		}

		public Result(T value)
		{
			this.ok = value;
			this.error = null;
		}

		public Result(IFormattedMessage errorMessage)
		{
			this.ok = default;
			this.error = errorMessage;
		}

		public static implicit operator Result<T>(T value)
		{
			return new Result<T>(value);
		}

		public static implicit operator Result<T>(ErrorResult error)
		{
			return new Result<T>(error.errorMessage);
		}
	}
}