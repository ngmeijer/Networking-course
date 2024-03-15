using shared;
using System;
using System.Net.Sockets;
using System.Text;

/**
 * This class implements a simple TCP Echo Client.
 */
class TCPClientSample
{
	public static void Main (string[] args)
	{
		//Create a new TcpClient so we can setup a connection
		TcpClient client = new TcpClient();

		//Try to connect (ignoring any exceptions for now...) using the blocking Connect call
		Console.WriteLine("Connecting to server...");
		client.Connect ("localhost", 55555);
		Console.WriteLine("Connected to server.\n");

		//When we get here, we ARE connected, so we can get the stream 
		NetworkStream stream = client.GetStream ();

		while (true)
		{
			//Construct a string to send, convert it to bytes using UTF8 encoding and write it into the stream
			Console.WriteLine("Enter message to send:");
			string outString = Console.ReadLine();
			byte[] outBytes = Encoding.UTF8.GetBytes(outString);
			Console.WriteLine("Sending:" + outString);
			StreamUtil.Write(stream, outBytes);

			//and expect message of same size back (this call still blocks!)
			byte[] inBytes = StreamUtil.Read(stream);

			//print the message
			string inString = Encoding.UTF8.GetString(inBytes);
			Console.WriteLine("Received:" + inString);
			Console.WriteLine("");
		}

	}
}


