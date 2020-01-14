namespace Flow
{
	public struct InvalidTokenError : ICompileErrorMessage
	{
		public string Message() => "Invalid char";
	}
}