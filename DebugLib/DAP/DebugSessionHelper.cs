namespace Maestro.Debug
{
	public readonly struct DebugSessionHelper
	{
		private readonly bool clientLinesStartAt1;
		private readonly bool clientPathsAreUri;

		public DebugSessionHelper(Json.Value arguments)
		{
			clientLinesStartAt1 = arguments["linesStartAt1"].GetOr(true);

			switch (arguments["pathFormat"].wrapped)
			{
			case "uri": clientPathsAreUri = true; break;
			case "path": clientPathsAreUri = false; break;
			default: clientPathsAreUri = true; break;
			}
		}

		public int ConvertDebuggerLineToClient(int line)
		{
			return clientLinesStartAt1 ? line : line - 1;
		}

		public int ConvertClientLineToDebugger(int line)
		{
			return clientLinesStartAt1 ? line : line + 1;
		}

		public string ConvertDebuggerPathToClient(string path)
		{
			if (clientPathsAreUri)
			{
				try
				{
					var uri = new System.Uri(path);
					return uri.AbsoluteUri;
				}
				catch
				{
					return null;
				}
			}
			else
			{
				return path;
			}
		}

		public string ConvertClientPathToDebugger(string clientPath)
		{
			if (clientPath == null)
				return null;

			if (clientPathsAreUri)
			{
				if (System.Uri.IsWellFormedUriString(clientPath, System.UriKind.Absolute))
				{
					var uri = new System.Uri(clientPath);
					return uri.LocalPath;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return clientPath;
			}
		}
	}
}