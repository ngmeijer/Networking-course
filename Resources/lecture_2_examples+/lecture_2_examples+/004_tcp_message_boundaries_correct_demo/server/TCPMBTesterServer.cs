﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using shared;

class TCPMBTesterServer
{
	public static void Main (string[] args)
	{
		TcpListener listener = new TcpListener (IPAddress.Any, 55555);
		listener.Start ();

		TcpClient client = listener.AcceptTcpClient ();
		NetworkStream stream = client.GetStream ();

		while (true) {
			byte[] received = StreamUtil.Read (stream);

			Console.WriteLine ("Received {0} ({1} bytes)", Encoding.UTF8.GetString(received), received.Length);
		}

		stream.Close ();
		client.Close ();
		listener.Stop ();
	}
}


