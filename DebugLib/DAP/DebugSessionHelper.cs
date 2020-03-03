namespace Maestro.Debug
{
	internal readonly struct DebugSessionHelper
	{
		private readonly bool clientLinesStartAt1;
		private readonly bool clientColumnsStartAt1;
		private readonly bool clientPathsAreUri;

		public DebugSessionHelper(JsonValue arguments)
		{
			clientLinesStartAt1 = arguments["linesStartAt1"].GetOr(true);
			clientColumnsStartAt1 = arguments["columnsStartAt1"].GetOr(true);

			switch (arguments["pathFormat"].wrapped)
			{
			case "uri": clientPathsAreUri = true; break;
			case "path": clientPathsAreUri = false; break;
			default: clientPathsAreUri = true; break;
			}
		}

		public int ConvertDebuggerLineToClient(int line)
		{
			return clientLinesStartAt1 ? line + 1 : line;
		}

		public int ConvertClientLineToDebugger(int line)
		{
			return clientLinesStartAt1 ? line - 1 : line;
		}

		public int ConvertDebuggerColumnToClient(int column)
		{
			return clientColumnsStartAt1 ? column + 1 : column;
		}

		public int ConvertClientColumnToDebugger(int column)
		{
			return clientColumnsStartAt1 ? column - 1 : column;
		}

		public string ConvertDebuggerPathToClient(string path)
		{
			if (clientPathsAreUri)
			{
				if (System.Uri.TryCreate(path, System.UriKind.RelativeOrAbsolute, out var uri))
					return uri.AbsoluteUri;
				else
					return null;
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