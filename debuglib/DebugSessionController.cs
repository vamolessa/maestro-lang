namespace Maestro.Debug
{
	internal interface IDebugSession
	{
		void Initialize(DebugSessionController controller, Response response, Json.Value args);
		void Attach(DebugSessionController controller, Response response, Json.Value arguments);
		void Disconnect(DebugSessionController controller, Response response, Json.Value arguments);
		void SetBreakpoints(DebugSessionController controller, Response response, Json.Value arguments);
		void SetFunctionBreakpoints(DebugSessionController controller, Response response, Json.Value arguments);
		void SetExceptionBreakpoints(DebugSessionController controller, Response response, Json.Value arguments);
		void Continue(DebugSessionController controller, Response response, Json.Value arguments);
		void Next(DebugSessionController controller, Response response, Json.Value arguments);
		void StepIn(DebugSessionController controller, Response response, Json.Value arguments);
		void StepOut(DebugSessionController controller, Response response, Json.Value arguments);
		void Pause(DebugSessionController controller, Response response, Json.Value arguments);
		void StackTrace(DebugSessionController controller, Response response, Json.Value arguments);
		void Scopes(DebugSessionController controller, Response response, Json.Value arguments);
		void Variables(DebugSessionController controller, Response response, Json.Value arguments);
		void Source(DebugSessionController controller, Response response, Json.Value arguments);
		void Threads(DebugSessionController controller, Response response, Json.Value arguments);
		void Evaluate(DebugSessionController controller, Response response, Json.Value arguments);
	}

	internal sealed class DebugSessionController : ProtocolServer
	{
		private readonly IDebugSession session;
		private bool clientLinesStartAt1 = true;
		private bool clientPathsAreUri = true;

		public DebugSessionController(IDebugSession session)
		{
			this.session = session;
		}

		public void SendResponse(Response response, Json.Value body)
		{
			response.SetBody(body);
			SendMessage(response.Serialize());
		}

		public void SendErrorResponse(Response response, string errorMessage, Json.Value body = default)
		{
			response.SetErrorBody(errorMessage, body);
			SendMessage(response.Serialize());
		}

		protected override void DispatchRequest(string command, Json.Value args, Response response)
		{
			try
			{
				System.Console.WriteLine("DEBUGGER REQUEST: {0} ARGS:\n{1}", command, Json.Serialize(args));
				switch (command)
				{
				case "initialize":
					clientLinesStartAt1 = args["linesStartAt1"].GetOr(clientLinesStartAt1);
					switch (args["pathFormat"].wrapped)
					{
					case "uri":
						clientPathsAreUri = true;
						break;
					case "path":
						clientPathsAreUri = false;
						break;
					case string s:
						SendErrorResponse(response, $"initialize: bad value '{s}' for pathFormat");
						return;
					}
					session.Initialize(this, response, args);
					break;
				case "attach":
					session.Attach(this, response, args);
					break;
				case "disconnect":
					session.Disconnect(this, response, args);
					break;
				case "next":
					session.Next(this, response, args);
					break;
				case "continue":
					session.Continue(this, response, args);
					break;
				case "stepIn":
					session.StepIn(this, response, args);
					break;
				case "stepOut":
					session.StepOut(this, response, args);
					break;
				case "pause":
					session.Pause(this, response, args);
					break;
				case "stackTrace":
					session.StackTrace(this, response, args);
					break;
				case "scopes":
					session.Scopes(this, response, args);
					break;
				case "variables":
					session.Variables(this, response, args);
					break;
				case "source":
					session.Source(this, response, args);
					break;
				case "threads":
					session.Threads(this, response, args);
					break;
				case "setBreakpoints":
					session.SetBreakpoints(this, response, args);
					break;
				case "setFunctionBreakpoints":
					session.SetFunctionBreakpoints(this, response, args);
					break;
				case "setExceptionBreakpoints":
					session.SetExceptionBreakpoints(this, response, args);
					break;
				case "evaluate":
					session.Evaluate(this, response, args);
					break;
				default:
					SendErrorResponse(response, $"unrecognized request: {command}");
					break;
				}
			}
			catch (System.Exception e)
			{
				SendErrorResponse(response, $"error while processing request '{command}' (exception: {e.Message})");
			}
			finally
			{
				if (command is "disconnect")
					Stop();
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