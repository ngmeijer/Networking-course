using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;

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

			//get a packet
			byte[] inBytes = StreamUtil.Read(client.GetStream());
			Packet inPacket = new Packet(inBytes);

			//get the command
			string command = inPacket.ReadString();
			Console.WriteLine("Received command:" + command);

			//process it
			if (command == "addscore")
			{
				handleAddScore(client, inPacket);
			}
			else if (command == "getscores")
			{
				handleGetScores(client, inPacket);
			}
		}
	}

	private void handleAddScore(TcpClient pClient, Packet pInPacket)
	{
		_scores.Add(new Score(pInPacket.ReadString(), pInPacket.ReadInt()));
	}

	private void handleGetScores(TcpClient pClient, Packet pInPacket)
	{
		//construct a reply packet
		Packet outPacket = new Packet();

		outPacket.Write("highscores");
		outPacket.Write(_scores.Count);

		for (int i = 0; i < _scores.Count; i++)
		{
			outPacket.Write(_scores[i].name);
			outPacket.Write(_scores[i].score);
		}

		sendPacket(pClient, outPacket);
	}

	private void sendPacket(TcpClient pClient, Packet pOutPacket)
	{
		Console.WriteLine("Sending:" + pOutPacket);
		StreamUtil.Write(pClient.GetStream(), pOutPacket.GetBytes());
	}

}


