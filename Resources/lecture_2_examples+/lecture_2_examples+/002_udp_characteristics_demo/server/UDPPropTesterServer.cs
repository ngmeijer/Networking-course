using System;
using System.Net.Sockets;
using System.Net;

class UDPPropTesterServer
{
	public static void Main (string[] args)
	{
		UdpClient client = new UdpClient (55555);

		IPEndPoint endPoint = new IPEndPoint (IPAddress.Any, 0);

		int messageReceived = -1;
		int lastMessageReceived = -1;

		Console.WriteLine("Server started, waiting for messages...");

		while (true) {
			lastMessageReceived = messageReceived;

			byte[] receivePacket = client.Receive (ref endPoint);
			messageReceived = BitConverter.ToInt32 (receivePacket, 0);
			Console.WriteLine ("Received the number {0} ({1} bytes)", messageReceived, receivePacket.Length);

			//if this was the first message, don't do any checking
			bool firstMessage = (lastMessageReceived == -1);
			if (firstMessage) continue;

			//check for out of order packets
			if (messageReceived < lastMessageReceived) Console.WriteLine ("Out of order packet detected!");
			else if (messageReceived > lastMessageReceived + 1) Console.WriteLine ("Lost packet detected!");
			else if (messageReceived == lastMessageReceived) Console.WriteLine ("Duplicate packet detected!");
		}

		client.Close ();
	}
}


