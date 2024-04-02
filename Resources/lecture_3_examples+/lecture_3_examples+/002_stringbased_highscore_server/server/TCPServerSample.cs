using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;
using System.Text;

/**
 * This class implements a simple tcp highscore server.
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

	private void run() { 
		Console.WriteLine("Server started on port 55555");

		_listener = new TcpListener (IPAddress.Any, 55555);
		_listener.Start ();

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
			//only process client if it has any data
			if (client.Available == 0) continue;

			//get the 'request' as a string			
			byte[] inBytes = StreamUtil.Read(client.GetStream());
			string inString = Encoding.UTF8.GetString(inBytes);
			Console.WriteLine("Received:" + inString);

			//split the 'request' into it's parts and handle the request
			//note that if we pass in any weird request, eg addscore without additional params the server will crash!
			string[] inStringInParts = inString.Split(',');

			if (inStringInParts[0] == "addscore")
			{
				handleAddScore(client, inStringInParts);
			}
			else if (inStringInParts[0] == "getscores")
			{
				handleGetScores(client, inStringInParts);
			}
		}
	}

	private void handleAddScore(TcpClient pClient, string[] inStringInParts)
	{
		_scores.Add(new Score(inStringInParts[1], int.Parse(inStringInParts[2])));
	}

	private void handleGetScores(TcpClient pClient, string[] inStringInParts)
	{
		//construct a reply string
		string outString = "highscores," + _scores.Count;

		for (int i = 0; i < _scores.Count; i++)
		{
			outString += "," + _scores[i].name + "," + _scores[i].score;
		}

		sendString(pClient, outString);
	}

	private void sendString (TcpClient pClient, string pOutString)
	{
		Console.WriteLine("Sending:"+pOutString);
		StreamUtil.Write(pClient.GetStream(), Encoding.UTF8.GetBytes(pOutString));
	}

}


