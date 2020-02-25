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

		private bool TryMatchSourcePath(string sourceUri, out string sourcePath)
		{
			for (var i = 0; i < breakpoints.count; i++)
			{
				sourcePath = breakpoints.buffer[i].sourcePath;
				if (PathHelper.EndsWith(sourcePath, sourceUri))
					return true;
			}

			sourcePath = null;
			return false;
		}

		private void OnRequest(Request request, Json.Value arguments)
		{
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
						{"supportsSetVariable", true},
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

				if (state == State.Paused)
					SendStoppedEvent("entry");
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
				SendStoppedEvent("pause");
				break;
			case "stackTrace":
				{
					var stackFrames = new Json.Array();
					lock (this)
					{
						for (var i = vm.stackFrames.count - 1; i >= 1; i--)
						{
							var frame = vm.stackFrames.buffer[i];
							var assembly = frame.executable.assembly;
							var commandName = "<entry-point>";
							if (frame.commandIndex >= 0)
								commandName = assembly.commandDefinitions.buffer[frame.commandIndex].name;
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
								{"name", commandName},
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
						for (var i = breakpoints.count - 1; i >= 0; i--)
						{
							if (breakpoints.buffer[i].sourcePath == sourcePath)
								breakpoints.SwapRemove(i);
						}

						foreach (var bp in arguments["breakpoints"])
						{
							if (!bp["line"].TryGet(out int line))
								continue;

							breakpoints.PushBack(new Breakpoint(sourcePath, line));

							bps.Add(new Json.Object {
								{"verified", true},
								{"line", line}
							});
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
			case "setVariable":
				{
					var name = arguments["name"].GetOr("");
					var value = arguments["value"].GetOr("");

					var success = false;
					for (var i = vm.stackFrames.count - 1; i >= 0; i--)
					{
						var variableIndex = vm.FindVariableIndex(i);
						if (variableIndex.isSome)
						{
							var variableInfo = vm.debugInfo.variableInfos.buffer[variableIndex.value];
							if (variableInfo.name != name)
								continue;

							if (!Json.TryDeserialize(value, out var parsedValue))
								break;

							var newValue = new Value();
							success = true;
							switch (parsedValue.wrapped)
							{
							case null: break;
							case bool b: newValue = new Value(b); break;
							case int n: newValue = new Value(n); break;
							case float f: newValue = new Value(f); break;
							case string s: newValue = new Value(s); break;
							default: success = false; break;
							}

							if (success)
							{
								vm.stack.buffer[variableInfo.stackIndex] = newValue;
								var sb = new StringBuilder();
								newValue.AppendTo(sb);
								server.SendResponse(request, new Json.Object {
									{"value", sb.ToString()},
									{"type", newValue.GetTypeName()},
								});
							}
							break;
						}
					}

					if (!success)
						server.SendErrorResponse(request, $"could not parse new value '{value}' for variable {name}");
				}
				break;
			default:
				server.SendErrorResponse(request, $"could not handle request '{request}'");
				break;
			}
		}
	}
}
