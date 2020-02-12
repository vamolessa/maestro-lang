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
	}

	public sealed class SourceCollection
	{
		private Buffer<Source> sources = new Buffer<Source>();

		public void AddSource(Source source)
		{
			if (!GetSource(source.uri).isSome)
				sources.PushBack(source);
		}

		public Option<Source> GetSource(string uri)
		{
			for (var i = 0; i < sources.count; i++)
			{
				var source = sources.buffer[i];
				if (source.uri == uri)
					return source;
			}

			return Option.None;
		}
	}
}