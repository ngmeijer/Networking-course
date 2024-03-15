using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;

class UDPMBTestClient
{
	public static void Main (string[] args)
	{
		UdpClient client = new UdpClient ();

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
			client.Send (sendPacket, sendPacket.Length, "localhost", 55555);
			Thread.Sleep (500);
		}

		client.Close ();
	}
}


