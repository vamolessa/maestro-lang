namespace Maestro.Debug
{
	public sealed partial class Debugger
	{
		private void OnRequest(Request request, Json.Value arguments)
		{
			System.Console.WriteLine("DEBUGGER REQUEST: {0} [{1}] ARGS:\n{2}", request.command, request.seq, Json.Serialize(arguments));

			switch (request.command)
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

					server.SendResponse(request, capabilities);
					server.SendEvent("initialized");

					isConnected = true;
					if (isDebugging)
					{
						server.SendEvent("stopped", new Json.Object {
							{"reason", "entry"},
							{"description", "Paused on entry"},
						});
					}
				}
				break;
			case "attach":
				server.SendResponse(request);
				break;
			case "disconnect":
				isConnected = false;
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
					var startIndex = arguments["startFrame"].GetOr(0);
					var count = arguments["levels"].GetOr(0);

					var sfs = new Json.Array();
					lock (this)
					{
						if (count == 0)
							count = vm.stackFrames.count;
						var endIndex = System.Math.Min(vm.stackFrames.count, startIndex + count);

						for (var i = startIndex; i < endIndex; i++)
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

							sfs.Add(new Json.Object {
								{"id", i},
								{"name", command.name},
								{"line", helper.ConvertDebuggerLineToClient(pos.lineIndex)},
								{"column", helper.ConvertDebuggerColumnToClient(pos.columnIndex)},
								{"source", responseSourceObject},
							});
						}
					}
					server.SendResponse(request);
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
				server.SendResponse(request);
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
