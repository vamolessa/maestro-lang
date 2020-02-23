using System.IO;
using System.Net;
using System.Threading;

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

		private Buffer<SourcePosition> breakpoints = new Buffer<SourcePosition>();

		private VirtualMachine vm;
		private ByteCodeChunk chunk;
		private ConnectionState connectionState = ConnectionState.Disconnected;
		private State state = State.Paused;
		private SourcePosition lastPosition = new SourcePosition();

		public Debugger(ushort port)
		{
			server = new ProtocolServer(OnRequest);
			server.Start(IPAddress.Parse("127.0.0.1"), port);
		}

		void IDebugger.OnBegin(VirtualMachine vm, ByteCodeChunk chunk)
		{
			this.vm = vm;
			this.chunk = chunk;

			lock (this)
			{
				state = State.Paused;
				switch (connectionState)
				{
				case ConnectionState.Disconnected:
					connectionState = ConnectionState.WaitingClient;
					break;
				case ConnectionState.WaitingDebugger:
					connectionState = ConnectionState.Connected;
					SendStoppedEvent("entry");
					break;
				}
			}
		}

		void IDebugger.OnEnd(VirtualMachine vm, ByteCodeChunk chunk)
		{
			lock (this)
			{
				switch (connectionState)
				{
				case ConnectionState.Connected:
					connectionState = ConnectionState.WaitingDebugger;
					break;
				case ConnectionState.WaitingDebugger:
					connectionState = ConnectionState.Disconnected;
					break;
				}
			}
		}

		void IDebugger.OnHook(VirtualMachine vm, ByteCodeChunk chunk)
		{
			var codeIndex = vm.stackFrames.buffer[vm.stackFrames.count - 1].codeIndex;
			if (codeIndex < 0)
				return;
			var sourceIndex = chunk.FindSourceIndex(codeIndex);
			if (sourceIndex < 0)
				return;

			var source = chunk.sources.buffer[sourceIndex];
			var slice = chunk.sourceSlices.buffer[codeIndex];
			var line = (ushort)(FormattingHelper.GetLineAndColumn(source.content, slice.index).lineIndex + 1);
			var position = new SourcePosition(source.uri, line);

			switch (state)
			{
			case State.Continuing:
				lock (this)
				{
					for (var i = 0; i < breakpoints.count; i++)
					{
						var breakpoint = breakpoints.buffer[i];
						var wasOnBreakpoint =
							// lastPosition.sourceUri == breakpoint.sourceUri &&
							lastPosition.line == breakpoint.line;

						if (!wasOnBreakpoint && position.line == breakpoint.line)
						{
							state = State.Paused;
							System.Console.WriteLine("SEND STOPPED FORM CONTINUING AT LINE {0}", position.line);
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
						System.Console.WriteLine("SEND STOPPED FORM STEPPING");
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

		private bool TryMatchSourcePath(string sourceUri, out string sourcePath)
		{
			sourceUri = sourceUri.Replace(Path.AltDirectorySeparatorChar, Path.PathSeparator);
			for (var i = 0; i < breakpoints.count; i++)
			{
				sourcePath = breakpoints.buffer[i].sourceUri;
				sourcePath = sourcePath.Replace(Path.AltDirectorySeparatorChar, Path.PathSeparator);

				if (sourcePath.EndsWith(sourceUri))
					return true;
			}

			sourcePath = null;
			return false;
		}
	}
}