using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

class TCPMBTesterServer
{
	public static void Main (string[] args)
	{
		TcpListener listener = new TcpListener (IPAddress.Any, 55555);
		listener.Start ();

		TcpClient client = listener.AcceptTcpClient ();
		NetworkStream stream = client.GetStream ();
		byte[] buffer = new byte[1024];

		while (true) {
			int bytesReceived = stream.Read (buffer, 0, buffer.Length);

			Console.WriteLine (
				"Received {0} ({1} bytes)", 
				Encoding.UTF8.GetString(buffer, 0, bytesReceived), 
				bytesReceived
			);
		}

		stream.Close ();
		client.Close ();
		listener.Stop ();
	}
}


