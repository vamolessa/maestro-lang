namespace Maestro.Debug
{
	public sealed partial class Debugger
	{
		private void SendStoppedEvent(string reason)
		{
			server.SendEvent("stopped", new Json.Object {
				{"reason", reason},
				{"description", "Paused on " + reason},
				{"threadId", 1},
			});
		}

		private void OnRequest(Request request, Json.Value arguments)
		{
			System.Console.WriteLine("DEBUGGER REQUEST: {0} [{1}] ARGS: {2}", request.command, request.seq, Json.Serialize(arguments));

			switch (request.command)
			{
			case "initialize":
				{
					helper = new DebugSessionHelper(arguments);
					server.SendResponse(request, new Json.Object{
						{"supportsConfigurationDoneRequest", true},
						{"supportsFunctionBreakpoints", false},
						{"supportsConditionalBreakpoints", false},
						{"supportsHitConditionalBreakpoints", false},
						{"supportsEvaluateForHovers", false},
						{"exceptionBreakpointFilters", new Json.Array()},
						{"supportsStepBack", false},
						{"supportsSetVariable", false},
						{"supportsRestartFrame", false},
						{"supportsGotoTargetsRequest", false},
						{"supportsStepInTargetsRequest", false},
						{"supportsCompletionsRequest", false},
						{"completionTriggerCharacters", false},
						{"supportsModulesRequest", false},
						{"additionalModuleColumns", false},
						{"supportedChecksumAlgorithms", false},
						{"supportsRestartRequest", false},
						{"supportsExceptionOptions", false},
						{"supportsValueFormattingOptions", false},
						{"supportTerminateDebuggee", false},
						{"supportsDelayedStackTraceLoading", false},
						{"supportsLoadedSourcesRequest", false},
						{"supportsLogPoints", false},
						{"supportsTerminateThreadsRequest", false},
						{"supportsSetExpression", false},
						{"supportsTerminateRequest", false},
						{"supportsDataBreakpoints", false},
						{"supportsReadMemoryRequest", false},
						{"supportsDisassembleRequest", false},
						{"supportsCancelRequest", false},
						{"supportsBreakpointLocationsRequest", false},
					});

					server.SendEvent("initialized");
				}
				break;
			case "configurationDone":
				server.SendResponse(request);
				lock (this)
				{
					switch (connectionState)
					{
					case ConnectionState.Disconnected:
						connectionState = ConnectionState.WaitingDebugger;
						break;
					case ConnectionState.WaitingClient:
						System.Console.WriteLine("SEND STOPPED FROM CONFIGURATION DONE");
						connectionState = ConnectionState.Connected;
						SendStoppedEvent("entry");
						break;
					}
				}
				break;
			case "attach":
				server.SendResponse(request);
				break;
			case "disconnect":
				lock (this)
				{
					switch (connectionState)
					{
					case ConnectionState.Connected:
						connectionState = ConnectionState.WaitingClient;
						break;
					case ConnectionState.WaitingClient:
						connectionState = ConnectionState.Disconnected;
						break;
					}
				}
				server.Stop();
				server.SendResponse(request);
				break;
			case "next":
				lock (this)
				{
					state = State.Stepping;
				}
				server.SendResponse(request);
				break;
			case "continue":
				lock (this)
				{
					state = State.Continuing;
				}
				server.SendResponse(request);
				break;
			case "stepIn":
				lock (this)
				{
					state = State.Stepping;
				}
				server.SendResponse(request);
				break;
			case "stepOut":
				lock (this)
				{
					state = State.Stepping;
				}
				server.SendResponse(request);
				break;
			case "pause":
				lock (this)
				{
					state = State.Paused;
				}
				server.SendResponse(request);
				break;
			case "stackTrace":
				{
					var stackFrames = new Json.Array();
					lock (this)
					{
						for (var i = vm.stackFrames.count - 1; i >= 1; i--)
						{
							var frame = vm.stackFrames.buffer[i];
							var command = chunk.commandDefinitions.buffer[frame.commandIndex];
							var codeIndex = System.Math.Max(frame.codeIndex - 1, 0);
							var sourceContentIndex = chunk.sourceSlices.buffer[codeIndex].index;
							var sourceIndex = chunk.FindSourceIndex(codeIndex);
							var source = chunk.sources.buffer[sourceIndex];
							var pos = FormattingHelper.GetLineAndColumn(
								source.content,
								sourceContentIndex
							);

							var responseSourceObject = new Json.Object();
							if (TryMatchSourcePath(source.uri, out var sourcePath))
								responseSourceObject.Add("path", sourcePath);
							else
								responseSourceObject.Add("sourceReference", sourceIndex + 1);

							stackFrames.Add(new Json.Object {
								{"id", i},
								{"name", command.name},
								{"source", responseSourceObject},
								{"line", helper.ConvertDebuggerLineToClient(pos.lineIndex)},
								{"column", helper.ConvertDebuggerColumnToClient(pos.columnIndex)},
							});
						}
					}

					server.SendResponse(request, new Json.Object{
						{"stackFrames", stackFrames}
					});
				}
				break;
			case "scopes":
				server.SendResponse(request);
				break;
			case "variables":
				server.SendResponse(request);
				break;
			case "source":
				{
					if (!arguments["source"]["sourceReference"].TryGet(out int reference))
					{
						server.SendErrorResponse(request, $"could not load source");
						return;
					}

					var source = chunk.sources.buffer[reference - 1];
					server.SendResponse(request, new Json.Object{
						{"content", source.content}
					});
				}
				break;
			case "threads":
				server.SendResponse(request, new Json.Object {
					{"threads", new Json.Array{
						new Json.Object{
							{"id", 1},
							{"name", "main"},
						},
					}}
				});
				break;
			case "setBreakpoints":
				{
					var sourceName = arguments["source"]["name"].GetOr("");
					var sourcePath = arguments["source"]["path"].GetOr("");

					var bps = new Json.Array();
					lock (this)
					{
						foreach (var bp in arguments["breakpoints"])
						{
							if (!bp["line"].TryGet(out int line))
								continue;

							bps.Add(new Json.Object {
								{"verified", true},
								{"line", line}
							});

							breakpoints.PushBack(new SourcePosition(sourcePath, line));
						}
					}

					server.SendResponse(request, new Json.Object {
						{"breakpoints", bps}
					});
				}
				break;
			case "setFunctionBreakpoints":
				server.SendResponse(request);
				break;
			case "setExceptionBreakpoints":
				server.SendResponse(request);
				break;
			case "evaluate":
				server.SendResponse(request);
				break;
			default:
				server.SendErrorResponse(request, $"could not handle request '{request}'");
				break;
			}
		}
	}
}
