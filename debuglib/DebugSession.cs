namespace Maestro.Debug
{
	internal abstract class DebugSession : ProtocolServer
	{
		private bool clientLinesStartAt1 = true;
		private bool clientPathsAreURI = true;

		public void SendResponse(Response response, Json.Value body)
		{
			response.SetBody(body);
			SendMessage(response.Serialize());
		}

		public void SendErrorResponse(Response response, string errorMessage)
		{
			response.SetErrorBody(errorMessage, default);
			SendMessage(response.Serialize());
		}

		protected override void DispatchRequest(string command, Json.Value args, Response response)
		{
			try
			{
				switch (command)
				{
				case "initialize":
					clientLinesStartAt1 = args["linesStartAt1"].GetOr(clientLinesStartAt1);
					if (args["pathFormat"].TryGet(out string pathFormat))
					{
						switch (pathFormat)
						{
						case "uri":
							clientPathsAreURI = true;
							break;
						case "path":
							clientPathsAreURI = false;
							break;
						default:
							SendErrorResponse(response, $"initialize: bad value '{pathFormat}' for pathFormat");
							return;
						}
					}
					Initialize(response, args);
					break;
				case "attach":
					Attach(response, args);
					break;
				case "disconnect":
					Disconnect(response, args);
					break;
				case "next":
					Next(response, args);
					break;
				case "continue":
					Continue(response, args);
					break;
				case "stepIn":
					StepIn(response, args);
					break;
				case "stepOut":
					StepOut(response, args);
					break;
				case "pause":
					Pause(response, args);
					break;
				case "stackTrace":
					StackTrace(response, args);
					break;
				case "scopes":
					Scopes(response, args);
					break;
				case "variables":
					Variables(response, args);
					break;
				case "source":
					Source(response, args);
					break;
				case "threads":
					Threads(response, args);
					break;
				case "setBreakpoints":
					SetBreakpoints(response, args);
					break;
				case "setFunctionBreakpoints":
					SetFunctionBreakpoints(response, args);
					break;
				case "setExceptionBreakpoints":
					SetExceptionBreakpoints(response, args);
					break;
				case "evaluate":
					Evaluate(response, args);
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

		public abstract void Initialize(Response response, Json.Value args);
		public abstract void Attach(Response response, Json.Value arguments);
		public abstract void Disconnect(Response response, Json.Value arguments);

		public virtual void SetFunctionBreakpoints(Response response, Json.Value arguments)
		{
		}

		public virtual void SetExceptionBreakpoints(Response response, Json.Value arguments)
		{
		}

		public abstract void SetBreakpoints(Response response, Json.Value arguments);

		public abstract void Continue(Response response, Json.Value arguments);

		public abstract void Next(Response response, Json.Value arguments);

		public abstract void StepIn(Response response, Json.Value arguments);

		public abstract void StepOut(Response response, Json.Value arguments);

		public abstract void Pause(Response response, Json.Value arguments);

		public abstract void StackTrace(Response response, Json.Value arguments);

		public abstract void Scopes(Response response, Json.Value arguments);

		public abstract void Variables(Response response, Json.Value arguments);

		public abstract void Source(Response response, Json.Value arguments);

		public abstract void Threads(Response response, Json.Value arguments);

		public abstract void Evaluate(Response response, Json.Value arguments);
	}
}