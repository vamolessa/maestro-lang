using System.IO;

namespace Maestro.Debug
{
	public static class PathHelper
	{
		public static bool IsDirectorySeparator(char c)
		{
			return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
		}

		public static int GetLengthWithoutExtension(string path)
		{
			var extensionIndex = path.LastIndexOf('.');
			var lastSeparatorIndex = System.Math.Max(
				path.LastIndexOf(Path.DirectorySeparatorChar),
				path.LastIndexOf(Path.AltDirectorySeparatorChar)
			);

			return extensionIndex > lastSeparatorIndex ?
				extensionIndex :
				path.Length;
		}

		public static bool EndsWith(string path, string match)
		{
			var pathLength = GetLengthWithoutExtension(path);
			var matchLength = GetLengthWithoutExtension(match);

			if (matchLength > pathLength)
				return false;

			while (matchLength > 0)
			{
				var c = path[--pathLength];
				var m = match[--matchLength];

				if (IsDirectorySeparator(c))
				{
					if (!IsDirectorySeparator(m))
						return false;
				}
				else if (c != m)
				{
					return false;
				}
			}

			return true;
		}
	}
}