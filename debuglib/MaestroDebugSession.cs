namespace Maestro.Debug
{
	internal sealed class MaestroDebugSession : DebugSession
	{
		public override void Initialize(Response response, Json.Value args)
		{
			var capabilities = Json.Value.NewObject();

			// This debug adapter does not need the configurationDoneRequest.
			capabilities["supportsConfigurationDoneRequest"] = false;

			// This debug adapter does not support function breakpoints.
			capabilities["supportsFunctionBreakpoints"] = false;

			// This debug adapter doesn't support conditional breakpoints.
			capabilities["supportsConditionalBreakpoints"] = false;

			// This debug adapter does not support a side effect free evaluate request for data hovers.
			capabilities["supportsEvaluateForHovers"] = false;

			// This debug adapter does not support exception breakpoint filters
			capabilities["exceptionBreakpointFilters"] = Json.Value.NewArray();

			SendResponse(response, capabilities);

			// Mono Debug is ready to accept breakpoints immediately
			SendEvent("initialized");
		}

		public override void Attach(Response response, Json.Value arguments)
		{
		}

		public override void Disconnect(Response response, Json.Value arguments)
		{
		}

		public override void SetBreakpoints(Response response, Json.Value arguments)
		{
		}

		public override void Continue(Response response, Json.Value arguments)
		{
		}

		public override void Next(Response response, Json.Value arguments)
		{
		}

		public override void StepIn(Response response, Json.Value arguments)
		{
		}

		public override void StepOut(Response response, Json.Value arguments)
		{
		}

		public override void Pause(Response response, Json.Value arguments)
		{
		}

		public override void StackTrace(Response response, Json.Value arguments)
		{
		}

		public override void Scopes(Response response, Json.Value arguments)
		{
		}

		public override void Variables(Response response, Json.Value arguments)
		{
		}

		public override void Source(Response response, Json.Value arguments)
		{
		}

		public override void Threads(Response response, Json.Value arguments)
		{
		}

		public override void Evaluate(Response response, Json.Value arguments)
		{
		}
	}
}