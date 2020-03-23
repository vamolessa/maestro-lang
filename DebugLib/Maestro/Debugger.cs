using System.Net;
using System.Threading;

using PicoJson.Untyped;

namespace Maestro.Debug
{
	public readonly struct SourcePosition
	{
		public readonly string sourceUri;
		public readonly int line;

		public SourcePosition(string sourceUri, int line)
		{
			this.sourceUri = sourceUri;
			this.line = line;
		}
	}

	public readonly struct Breakpoint
	{
		public readonly string sourcePath;
		public readonly int line;

		public Breakpoint(string sourcePath, int line)
		{
			this.sourcePath = sourcePath;
			this.line = line;
		}
	}

	public sealed partial class Debugger : IDebugger
	{
		private enum State
		{
			Paused,
			Continuing,
			Stepping,
		}

		private enum ConnectionState
		{
			Disconnected,
			WaitingDebugger,
			WaitingClient,
			Connected,
		}

		private readonly ProtocolServer server;
		private DebugSessionHelper helper = default;

		private Buffer<Breakpoint> breakpoints = new Buffer<Breakpoint>();

		private VirtualMachine vm;
		private bool isConnected = false;
		private State state = State.Paused;
		private SourcePosition lastPosition = new SourcePosition();

		public Debugger()
		{
			server = new ProtocolServer(OnRequest);
		}

		public void Start(ushort port)
		{
			server.Start(IPAddress.Loopback, port);
		}

		public void Start(IPAddress address, ushort port)
		{

			server.Start(address, port);
		}

		public void Stop()
		{
			server.Stop();
		}

		public void WaitForClient()
		{
			while (true)
			{
				lock (this)
				{
					if (isConnected)
						break;
				}

				Thread.Sleep(1000);
			}
		}

		void IDebugger.OnBegin(VirtualMachine vm)
		{
			this.vm = vm;

			lock (this)
			{
				state = State.Continuing;
			}
		}

		void IDebugger.OnEnd(VirtualMachine vm)
		{
		}

		void IDebugger.OnHook(VirtualMachine vm)
		{
			var frame = vm.stackFrames.buffer[vm.stackFrames.count - 1];
			var assembly = frame.executable.assembly;
			var codeIndex = frame.codeIndex;
			if (codeIndex < 0)
				return;

			var slice = assembly.sourceSlices.buffer[codeIndex];
			var line = (ushort)(FormattingHelper.GetLineAndColumn(assembly.source.content, slice.index).lineIndex + 1);
			var position = new SourcePosition(assembly.source.uri, line);

			switch (state)
			{
			case State.Continuing:
				lock (this)
				{
					for (var i = 0; i < breakpoints.count; i++)
					{
						var breakpoint = breakpoints.buffer[i];
						if (
							(lastPosition.sourceUri != position.sourceUri ||
							lastPosition.line != position.line) &&
							position.line == breakpoint.line &&
							PathHelper.EndsWith(breakpoint.sourcePath, position.sourceUri)
						)
						{
							state = State.Paused;
							SendStoppedEvent("breakpoint");
							break;
						}
					}
				}
				break;
			case State.Stepping:
				if (lastPosition.sourceUri != position.sourceUri || lastPosition.line != position.line)
				{
					lock (this)
					{
						state = State.Paused;
						SendStoppedEvent("step");
					}
					break;
				}
				break;
			}

			while (true)
			{
				lock (this)
				{
					if (state != State.Paused)
						break;
				}

				Thread.Sleep(1000);
			}

			lastPosition = position;
		}
	}
}