using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Maestro.Debug
{
	public sealed class Debugger : IDebugger, IDebugSession
	{
		public Debugger(ushort port)
		{
			StartServer(port);
		}

		private void StartServer(ushort port)
		{
			var serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
			serverSocket.Start();

			var serverThread = new Thread(() => {
				while (true)
				{
					var clientSocket = serverSocket.AcceptSocket();
					if (clientSocket == null)
						continue;

					var clientThread = new Thread(() => {
						using (var stream = new NetworkStream(clientSocket))
						{
							try
							{
								var controller = new DebugSessionController(this);
								controller.Start(stream, stream);
							}
							catch { }
						}
						clientSocket.Close();
					});
					clientThread.IsBackground = true;
					clientThread.Start();
				}
			});
			serverThread.IsBackground = true;
			serverThread.Start();
		}

		void IDebugSession.Initialize(DebugSessionController controller, Response response, Json.Value args)
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

			controller.SendResponse(response, capabilities);
			controller.SendEvent("initialized");
		}

		void IDebugSession.Attach(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Disconnect(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.SetBreakpoints(DebugSessionController controller, Response response, Json.Value arguments)
		{
			var sourceName = arguments["source"]["name"].GetOr("");
			var sourcePath = arguments["source"]["path"].GetOr("");

			foreach (var breakpoint in arguments["breakpoints"])
			{
				var line = breakpoint["line"].GetOr(0);
				System.Console.WriteLine("BREAKPOINT ON {0} AT LINE {1}", sourceName, line);
			}
		}

		void IDebugSession.SetFunctionBreakpoints(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.SetExceptionBreakpoints(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Continue(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Next(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.StepIn(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.StepOut(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Pause(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.StackTrace(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Scopes(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Variables(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Source(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Threads(DebugSessionController controller, Response response, Json.Value arguments)
		{
		}

		void IDebugSession.Evaluate(DebugSessionController controller, Response response, Json.Value arguments)
		{
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