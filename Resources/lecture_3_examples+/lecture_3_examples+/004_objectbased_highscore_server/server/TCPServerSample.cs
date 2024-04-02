using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;

/**
 * This class implements a simple tcp highscore server.
 * Read carefully through the comments below.
 * Note that the server does not contain any sort of error handling.
 */
class TCPServerSample
{
	public static void Main(string[] args)
	{
		TCPServerSample server = new TCPServerSample();
		server.run();
	}

	private TcpListener _listener;
	private List<TcpClient> _clients = new List<TcpClient>();
	private List<Score> _scores = new List<Score>();

	private void run()
	{
		Console.WriteLine("Server started on port 55555");

		_listener = new TcpListener(IPAddress.Any, 55555);
		_listener.Start();

		while (true)
		{
			processNewClients();
			processExistingClients();

			//Although technically not required, now that we are no longer blocking, 
			//it is good to cut your CPU some slack
			Thread.Sleep(100);
		}
	}

	private void processNewClients()
	{
		while (_listener.Pending())
		{
			_clients.Add(_listener.AcceptTcpClient());
			Console.WriteLine("Accepted new client.");
		}
	}

	private void processExistingClients()
	{
		foreach (TcpClient client in _clients)
		{
			if (client.Available == 0) continue;

			byte[] inBytes = StreamUtil.Read(client.GetStream());
			Packet inPacket = new Packet(inBytes);
			ISerializable inObject = inPacket.ReadObject();
			Console.WriteLine("Received:"+inObject);

			if (inObject is AddRequest addRequest)		{	handleAddRequest(client, addRequest);	}
			else if (inObject is GetRequest getRequest)	{	handleGetRequest(client, getRequest);	}
		}
	}

	private void handleAddRequest(TcpClient pClient, AddRequest pAddRequest)
	{
		_scores.Add(pAddRequest.score);
	}

	private void handleGetRequest(TcpClient pClient, GetRequest pGetRequest)
	{
		//construct a reply 
		HighscoresUpdate highscoreUpdate = new HighscoresUpdate();
		highscoreUpdate.scores = _scores;
		sendObject(pClient, highscoreUpdate);
	}

	private void sendObject(TcpClient pClient, ISerializable pOutObject)
	{
		Console.WriteLine("Sending:"+pOutObject);
		Packet outPacket = new Packet();
		outPacket.Write(pOutObject);
		StreamUtil.Write(pClient.GetStream(), outPacket.GetBytes());
	}

}

