using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Maestro.Debug
{
	internal class ProtocolMessage
	{
		public readonly string type;
		public int seq;

		public ProtocolMessage(string type, int seq)
		{
			this.type = type;
			this.seq = seq;
		}

		public ProtocolMessage(Json.Value value)
		{
			type = value[nameof(type)].GetOr("string");
			seq = value[nameof(seq)].GetOr(0);
		}

		public virtual Json.Value Serialize()
		{
			var value = Json.Value.NewObject();
			value[nameof(type)] = type;
			value[nameof(seq)] = seq;
			return value;
		}
	}

	internal sealed class Response : ProtocolMessage
	{
		public readonly int request_seq;
		public readonly string command;

		public bool success;
		public string message;
		public Json.Value body;

		public Response(Json.Value value) : base(value)
		{
			request_seq = value[nameof(request_seq)].GetOr(0);
			command = value[nameof(command)].GetOr("");

			success = value[nameof(success)].GetOr(false);
			message = value[nameof(message)].GetOr("");
			body = value[nameof(body)];
		}

		public Response(int request_seq, string command) : base("response", 0)
		{
			this.success = true;
			this.request_seq = seq;
			this.command = command;
		}

		public void SetBody(Json.Value body)
		{
			this.success = true;
			this.body = body;
		}

		public void SetErrorBody(string message, Json.Value body)
		{
			this.success = false;
			this.message = message;
			this.body = body;
		}

		public override Json.Value Serialize()
		{
			var value = base.Serialize();

			value[nameof(request_seq)] = request_seq;
			value[nameof(command)] = command;

			value[nameof(success)] = success;
			value[nameof(message)] = message;
			value[nameof(body)] = body;

			return value;
		}
	}

	internal abstract class ProtocolServer
	{
		protected const int BUFFER_SIZE = 4096;
		protected const string TWO_CRLF = "\r\n\r\n";
		protected static readonly Regex CONTENT_LENGTH_MATCHER = new Regex(@"Content-Length: (\d+)");

		protected static readonly Encoding Encoding = System.Text.Encoding.UTF8;

		private int sequenceNumber = 1;
		private Dictionary<int, TaskCompletionSource<Response>> pendingRequests = new Dictionary<int, TaskCompletionSource<Response>>();
		private bool stopRequested;

		private Stream outputStream;
		private ByteBuffer rawData = new ByteBuffer();
		private int bodyLength = -1;

		public void Start(Stream inputStream, Stream outputStream)
		{
			this.outputStream = outputStream;

			var buffer = new byte[BUFFER_SIZE];

			stopRequested = false;
			while (!stopRequested)
			{
				var read = inputStream.Read(buffer, 0, buffer.Length);
				if (read == 0)
					break;

				if (read > 0)
				{
					rawData.Append(buffer, read);
					ProcessData();
				}
			}
		}

		public void Stop()
		{
			stopRequested = true;
		}

		public void SendEvent(string eventName, Json.Value body = default)
		{
			var message = Json.Value.NewObject();
			message["event"] = eventName;
			if (body.wrapped != null)
				message["body"] = body;
			SendMessage(message);
		}

		protected abstract void DispatchRequest(string command, Json.Value args, Response response);

		private void ProcessData()
		{
			while (true)
			{
				if (bodyLength >= 0)
				{
					if (rawData.Length >= bodyLength)
					{
						var buf = rawData.RemoveFirst(bodyLength);
						bodyLength = -1;
						Dispatch(Encoding.GetString(buf));
						continue;
					}
				}
				else
				{
					var s = rawData.GetString(Encoding);
					var idx = s.IndexOf(TWO_CRLF);
					if (idx != -1)
					{
						var m = CONTENT_LENGTH_MATCHER.Match(s);
						if (m.Success && m.Groups.Count == 2)
						{
							bodyLength = System.Convert.ToInt32(m.Groups[1].ToString());
							rawData.RemoveFirst(idx + TWO_CRLF.Length);
							continue;
						}
					}
				}
				break;
			}
		}

		private void Dispatch(string req)
		{
			if (!Json.TryDeserialize(req, out var message))
				return;

			switch (message["type"].wrapped)
			{
			case "request":
				{
					var seq = message["seq"].GetOr(0);
					var command = message["command"].GetOr("");
					var arguments = message["arguments"];
					var response = new Response(seq, command);
					DispatchRequest(command, arguments, response);
					SendMessage(response.Serialize());
					break;
				}
			case "response":
				{
					var seq = message["request_seq"].GetOr(0);
					lock (pendingRequests)
					{
						if (pendingRequests.ContainsKey(seq))
						{
							var tcs = pendingRequests[seq];
							pendingRequests.Remove(seq);
							tcs.SetResult(new Response(message));
						}
					}
					break;
				}
			}
		}

		protected void SendMessage(Json.Value message)
		{
			if (message["seq"].GetOr(0) == 0)
				message["seq"] = sequenceNumber++;

			try
			{
				var data = ConvertToBytes(message);
				outputStream.Write(data, 0, data.Length);
				outputStream.Flush();
			}
			catch { }
		}

		private static byte[] ConvertToBytes(Json.Value message)
		{
			var asJson = Json.Serialize(message);
			var jsonBytes = Encoding.GetBytes(asJson);

			var header = string.Format("Content-Length: {0}{1}", jsonBytes.Length, TWO_CRLF);
			var headerBytes = Encoding.GetBytes(header);

			var data = new byte[headerBytes.Length + jsonBytes.Length];
			System.Buffer.BlockCopy(headerBytes, 0, data, 0, headerBytes.Length);
			System.Buffer.BlockCopy(jsonBytes, 0, data, headerBytes.Length, jsonBytes.Length);

			return data;
		}
	}

	internal sealed class ByteBuffer
	{
		private byte[] buffer = new byte[0];

		public int Length
		{
			get { return buffer.Length; }
		}

		public string GetString(Encoding enc)
		{
			return enc.GetString(buffer);
		}

		public void Append(byte[] b, int length)
		{
			var newBuffer = new byte[buffer.Length + length];
			System.Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
			System.Buffer.BlockCopy(b, 0, newBuffer, buffer.Length, length);
			buffer = newBuffer;
		}

		public byte[] RemoveFirst(int n)
		{
			var b = new byte[n];
			System.Buffer.BlockCopy(buffer, 0, b, 0, n);
			var newBuffer = new byte[buffer.Length - n];
			System.Buffer.BlockCopy(buffer, n, newBuffer, 0, buffer.Length - n);
			buffer = newBuffer;
			return b;
		}
	}
}
