using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace server
{
	class UDPEchoServer
	{
		public static void Main (string[] args)
		{
			//As a udp server, even though there is no persistent 'connection',
			//we still need to know where to send data to, so we have to specify a port
			UdpClient client = new UdpClient (55555);

			//When we actually DO receive something, we would like to know from where.
			//Since there is no persistent connection, this info might differ per 'read'.
			//To retrieve information about a sender, we use this endpoint instance,
			//where the passed in values do not matter, they will be overwritten by the actual values anyway.
			IPEndPoint endPoint = new IPEndPoint (IPAddress.Any, 0);
			Console.WriteLine ("Waiting for clients to serve...");

			while (true) {
				//Block until we receive a packet from ANYWHERE 
				//(we don't know where it will come from, multiple clients can send packets to 
				//this same receiving endpoint)
				byte[] inBytes = client.Receive (ref endPoint);
				string inString = Encoding.UTF8.GetString(inBytes);
				Console.WriteLine($"Received:{inString} ({inBytes.Length} bytes) from {endPoint}");

				//This server is a simple echo server, 
				//which means that we just 'echo' all the bytes we received back to the client
				Console.WriteLine($"Sending:{inString} ({inBytes.Length} bytes) to {endPoint}");
				client.Send (inBytes, inBytes.Length, endPoint);
			}
		}
	}
}


