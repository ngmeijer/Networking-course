using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

class TCPPropTesterServer
{
	public static void Main (string[] args)
	{
		TcpListener listener = new TcpListener (IPAddress.Any, 55555);
		listener.Start ();

		//we accept only one client, after the first client we stop functioning correctly
		TcpClient client = listener.AcceptTcpClient ();
		NetworkStream stream = client.GetStream ();

		int messageReceived = -1;
		int lastMessageReceived = -1;

		while (true) {
			lastMessageReceived = messageReceived;

			//this way of reading does not respect byte endian order or message boundaries, we'll get to that
			//in addition, exceptions and end of streams aren't handled correctly...
			byte[] receivePacket = new byte[4];
			int bytesRead = stream.Read(receivePacket, 0, 4);
			messageReceived = BitConverter.ToInt32 (receivePacket, 0);
			Console.WriteLine ("Received the number {0} ({1} bytes)", messageReceived, receivePacket.Length);

			//if this was the first message, don't do any checking
			bool firstMessage = (lastMessageReceived == -1);
			if (firstMessage) continue;

			//check for out of order packets
			if (messageReceived < lastMessageReceived) Console.WriteLine ("Out of order packet detected!");
			else if (messageReceived > lastMessageReceived+1) Console.WriteLine ("Lost packet detected!");
			else if (messageReceived == lastMessageReceived) Console.WriteLine ("Duplicate packet detected!");
		}

		//...todo cleanup sockets, streams and listener...
	}
}

