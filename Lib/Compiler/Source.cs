namespace Maestro
{
	public readonly struct Source
	{
		public readonly string uri;
		public readonly string content;

		public Source(string uri, string content)
		{
			this.uri = uri;
			this.content = content;
		}

		public bool HasContent
		{
			get { return !string.IsNullOrEmpty(content); }
		}
	}
}