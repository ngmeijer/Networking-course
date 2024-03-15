using shared;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TCPMBTesterClient
{
	public static void Main (string[] args)
	{
		TcpClient client = new TcpClient("localhost", 55555);
		NetworkStream stream = client.GetStream ();

		string[] messages = {
			"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
			"BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
			"CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
			"DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD",
			"EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE"
		};

		int messageIndex = -1;
		string message = null;

		while (true) {
			messageIndex = (messageIndex + 1) % messages.Length;
			message = messages [messageIndex];
			byte[] sendPacket = Encoding.UTF8.GetBytes(message);
			Console.WriteLine ("Sending  {0} ({1} bytes)", message, sendPacket.Length);
			StreamUtil.Write (stream, sendPacket);
			Thread.Sleep (300);
		}

		stream.Close ();
		client.Close ();
	}
}


