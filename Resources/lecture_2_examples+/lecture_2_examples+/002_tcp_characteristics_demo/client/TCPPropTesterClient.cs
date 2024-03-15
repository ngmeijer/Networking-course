using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;

class TCPPropTesterClient
{
	public static void Main (string[] args)
	{
		TcpClient client = new TcpClient();
		client.Connect ("localhost", 55555);

		NetworkStream stream = client.GetStream ();

		int messageNumber = 0;

		while (true) {
			byte[] sendPacket = BitConverter.GetBytes (++messageNumber);
			Console.WriteLine ("Sending the number {0} ({1} bytes)", messageNumber, sendPacket.Length);

			stream.Write (sendPacket, 0, sendPacket.Length);
			Thread.Sleep (10);
		}

		stream.Close ();
		client.Close ();
	}
}


