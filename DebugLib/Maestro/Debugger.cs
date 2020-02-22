using System.Net;

namespace Maestro.Debug
{
	public sealed class Debugger : IDebugger
	{
		private readonly ProtocolServer server;
		private DebugSessionHelper helper;

		public Debugger(ushort port)
		{
			server = new ProtocolServer(OnRequest);
			server.Start(IPAddress.Parse("127.0.0.1"), port);
		}

		private Json.Value OnRequest(string request, Json.Value arguments)
		{
			System.Console.WriteLine("DEBUGGER REQUEST: {0} ARGS:\n{1}", request, Json.Serialize(arguments));

			switch (request)
			{
			case "initialize":
				{
					helper = new DebugSessionHelper(arguments);
					var capabilities = new Json.Object{
						// This debug adapter does not need the configurationDoneRequest.
						{"supportsConfigurationDoneRequest", false},

						// This debug adapter does not support function breakpoints.
						{"supportsFunctionBreakpoints", false},

						// This debug adapter doesn't support conditional breakpoints.
						{"supportsConditionalBreakpoints", false},

						// This debug adapter does not support a side effect free evaluate request for data hovers.
						{"supportsEvaluateForHovers", false},

						// This debug adapter does not support exception breakpoint filters
						{"exceptionBreakpointFilters", new Json.Array()}
					};

					server.SendEvent("initialized");
					return ProtocolServer.Response(capabilities);
				}
			case "attach":
				return ProtocolServer.Response();
			case "disconnect":
				server.Stop();
				return ProtocolServer.Response();
			case "next":
				return ProtocolServer.Response();
			case "continue":
				return ProtocolServer.Response();
			case "stepIn":
				return ProtocolServer.Response();
			case "stepOut":
				return ProtocolServer.Response();
			case "pause":
				return ProtocolServer.Response();
			case "stackTrace":
				return ProtocolServer.Response();
			case "scopes":
				return ProtocolServer.Response();
			case "variables":
				return ProtocolServer.Response();
			case "source":
				return ProtocolServer.Response();
			case "threads":
				return ProtocolServer.Response();
			case "setBreakpoints":
				{
					var sourceName = arguments["source"]["name"].GetOr("");
					var sourcePath = arguments["source"]["path"].GetOr("");

					var breakpoints = new Json.Array();
					foreach (var breakpoint in arguments["breakpoints"])
					{
						var line = breakpoint["line"].GetOr(0);
						System.Console.WriteLine("BREAKPOINT ON {0} AT LINE {1}", sourceName, line);

						breakpoints.Add(new Json.Object {
							{"verified", true}
						});
					}

					return ProtocolServer.Response(new Json.Object {
						{"breakpoints", breakpoints}
					});
				}
			case "setFunctionBreakpoints":
				return ProtocolServer.Response();
			case "setExceptionBreakpoints":
				return ProtocolServer.Response();
			case "evaluate":
				return ProtocolServer.Response();
			default:
				return ProtocolServer.ErrorResponse($"invalid request '{request}'");
			}
		}

		void IDebugger.OnBegin(VirtualMachine vm)
		{
		}

		void IDebugger.OnEnd(VirtualMachine vm)
		{
		}

		void IDebugger.OnHook(VirtualMachine vm)
		{
		}
	}
}