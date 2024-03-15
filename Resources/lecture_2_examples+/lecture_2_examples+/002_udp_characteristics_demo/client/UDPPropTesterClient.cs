using System;
using System.Net.Sockets;
using System.Threading;

class UDPPropTesterClient
{
	public static void Main (string[] args)
	{
		UdpClient client = new UdpClient ();

		int messageNumber = 0;

		while (true) {
			byte[] sendPacket = BitConverter.GetBytes (++messageNumber);
			Console.WriteLine ("Sending the number {0} ({1} bytes)", messageNumber, sendPacket.Length);
			client.Send (sendPacket, sendPacket.Length, "localhost", 55555);
			Thread.Sleep (10);
		}

		client.Close ();
	}
}


