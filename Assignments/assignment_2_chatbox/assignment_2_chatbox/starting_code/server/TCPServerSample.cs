using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;

class TCPServerSample
{
	/**
	 * This class implements a simple concurrent TCP Echo server.
	 * Read carefully through the comments below.
	 */
	
	public static void Main (string[] args)
	{
		Console.WriteLine("Server started on port 55555");

		TcpListener listener = new TcpListener (IPAddress.Any, 55555);
		listener.Start ();

		Dictionary<string, TcpClient> clients= new Dictionary<string, TcpClient>();

		int clientIndex = 0;
		while (true)
		{
			processNewClients(listener, clientIndex, clients);
			processExistingClients(clients);
			cleanupFaultyClients(clients);

			//Although technically not required, now that we are no longer blocking, 
			//it is good to cut your CPU some slack
			Thread.Sleep(100);
		}
	}

	private static void processNewClients(TcpListener pListener, int pClientIndex, Dictionary<string,TcpClient> pClients)
	{
		//First big change with respect to example 001
		//We no longer block waiting for a client to connect, but we only block if we know
		//a client is actually waiting (in other words, we will not block)
		//In order to serve multiple clients, we add that client to a list
		while (pListener.Pending())
		{
			string clientName = $"Client_{pClientIndex}";

			TcpClient newClient = pListener.AcceptTcpClient();
			pClients.Add(clientName, newClient);

			//Log "joined session" message
			Console.WriteLine($"Accepted new client: {clientName}");
			NetworkStream stream = newClient.GetStream();
			string dataToSend = concatenateNameInData(stream);

			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
			
			StreamUtil.Write(stream, buffer);
			
			pClientIndex++;
		}	
	}

	private static void processExistingClients(Dictionary<string,TcpClient> pClients)
	{
		//Second big change, instead of blocking on one client, 
		//we now process all clients IF they have data available
		foreach (KeyValuePair<string, TcpClient> client in pClients)
		{
			if (client.Value.Available == 0) 
				continue;
			
			NetworkStream stream = client.Value.GetStream();

			byte[] receivedData = StreamUtil.Read(stream);
			string textRepresentation = System.Text.Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);
			string dataToSend = client.Key + ": " + textRepresentation;
			
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
			
			StreamUtil.Write(stream, buffer);
		}	
	}

	private static void cleanupFaultyClients(Dictionary<string,TcpClient> pClients)
	{
		Dictionary<string, TcpClient> tempClients = pClients;

		foreach (KeyValuePair<string, TcpClient> client in tempClients)
		{
			if (client.Value.Connected)
				continue;
			
			tempClients.Remove(client.Key);
		}

		pClients = tempClients;
	}

	private static string concatenateNameInData(NetworkStream pStream, string pClientName)
	{
		byte[] receivedData = StreamUtil.Read(pStream);
		string textRepresentation = System.Text.Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);
		return pClientName + ": " + textRepresentation;
	}

	private static void sendPublicMessage()
	{
		
	}
}


