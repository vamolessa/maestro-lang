using System.Text;

namespace Maestro.Debug
{
	public sealed partial class Debugger
	{
		private const int InputScopeOffset = 1000;

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
					server.SendResponse(request, new Json.Object {
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
					isConnected = true;
				}
				break;
			case "attach":
				server.SendResponse(request);
				break;
			case "disconnect":
				lock (this)
				{
					isConnected = false;
				}
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
							var assembly = frame.fatAssembly.assembly;
							var command = assembly.commandDefinitions.buffer[frame.commandIndex];
							var codeIndex = System.Math.Max(frame.codeIndex - 1, 0);
							var sourceContentIndex = assembly.sourceSlices.buffer[codeIndex].index;
							var pos = FormattingHelper.GetLineAndColumn(
								assembly.source.content,
								sourceContentIndex
							);

							var responseSource = new Json.Value();
							if (TryMatchSourcePath(assembly.source.uri, out var sourcePath))
							{
								responseSource = new Json.Object {
									{"path", sourcePath}
								};
							}

							stackFrames.Add(new Json.Object {
								{"id", i},
								{"name", command.name},
								{"source", responseSource},
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
				{
					var frameIndex = arguments["frameId"].GetOr(0);
					server.SendResponse(request, new Json.Object {
						{"scopes", new Json.Array {
							new Json.Object {
								{"name", "Input"},
								{"variablesReference", InputScopeOffset + frameIndex},
								{"expensive", false},
							},
							new Json.Object {
								{"name", "Locals"},
								{"variablesReference", frameIndex},
								{"expensive", false},
							},
						}}
					});
				}
				break;
			case "variables":
				{
					var frameIndex = arguments["variablesReference"].GetOr(0);
					var variables = new Json.Array();
					var sb = new StringBuilder();

					if (frameIndex >= InputScopeOffset)
					{
						frameIndex -= InputScopeOffset;
						var inputSlice = vm.inputSlices.buffer[frameIndex - 1];
						for (var i = 0; i < inputSlice.length; i++)
						{
							var value = vm.stack.buffer[inputSlice.index + i];
							sb.Clear();
							value.AppendTo(sb);

							variables.Add(new Json.Object {
								{"name", i.ToString()},
								{"value", sb.ToString()},
								{"type", value.GetTypeName()},
								{"variablesReference", 0},
							});
						}
					}
					else
					{
						var endStackIndex = vm.stack.count;
						if (frameIndex < vm.stackFrames.count - 1)
							endStackIndex = vm.stackFrames.buffer[frameIndex + 1].stackIndex;

						for (var i = 0; i < endStackIndex; i++)
						{
							var variableIndex = vm.FindVariableIndex(i);
							if (variableIndex.isSome)
							{
								var variableInfo = vm.debugInfo.variableInfos.buffer[variableIndex.value];
								var value = vm.stack.buffer[variableInfo.stackIndex];
								sb.Clear();
								value.AppendTo(sb);

								variables.Add(new Json.Object {
									{"name", variableInfo.name},
									{"value", sb.ToString()},
									{"type", value.GetTypeName()},
									{"variablesReference", 0},
								});
							}
						}
					}

					server.SendResponse(request, new Json.Object {
						{"variables", variables}
					});
				}
				break;
			case "source":
				server.SendResponse(request);
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
