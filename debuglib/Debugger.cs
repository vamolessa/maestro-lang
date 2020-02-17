using System.Net;
using System.Net.Sockets;

namespace Maestro.Debug
{
	public sealed class Debugger
	{
		private static void RunServer(int port)
		{
			var serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
			serverSocket.Start();

			new System.Threading.Thread(() => {
				while (true)
				{
					var clientSocket = serverSocket.AcceptSocket();
					if (clientSocket == null)
						continue;

					new System.Threading.Thread(() => {
						using (var stream = new NetworkStream(clientSocket))
						{
							try
							{
								// var debugSession = new DebugSession();
								// debugSession.Start(stream, stream);
							}
							catch { }
						}
						clientSocket.Close();
					}).Start();
				}
			}).Start();
		}
	}
}