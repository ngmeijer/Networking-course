using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

class UDPMBTestServer
{
	public static void Main (string[] args)
	{
		UdpClient client = new UdpClient (55555);

		IPEndPoint endPoint = new IPEndPoint (IPAddress.Any, 0);

		while (true) {
			byte[] receivePacket = client.Receive (ref endPoint);

			Console.WriteLine (
				"Received {0} ({1} bytes)", 
				Encoding.UTF8.GetString(receivePacket), 
				receivePacket.Length
			);
		}

		client.Close ();
	}
}


