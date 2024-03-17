using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using shared;
using System.Threading;

class TCPServerSample
{
	/**
	 * This class implements a simple concurrent TCP Echo server.
	 * Read carefully through the comments below.
	 */

	private static Dictionary<string, TcpClient> _clients = new Dictionary<string, TcpClient>();

	public static void Main (string[] args)
	{
		Console.WriteLine("Server started on port 55555");

		TcpListener listener = new TcpListener (IPAddress.Any, 55555);
		listener.Start ();
		
		int clientIndex = 0;
		while (true)
		{
			processNewClients(listener, clientIndex);
			processExistingClients();
			
			//Isn't it better to have this be the first call in the loop? Otherwise processExistingClients might run into disconnected clients.
			cleanupFaultyClients();

			//Although technically not required, now that we are no longer blocking, 
			//it is good to cut your CPU some slack
			Thread.Sleep(100);
		}
	}

	private static void processNewClients(TcpListener pListener, int pClientIndex)
	{
		//First big change with respect to example 001
		//We no longer block waiting for a client to connect, but we only block if we know
		//a client is actually waiting (in other words, we will not block)
		//In order to serve multiple clients, we add that client to a list
		while (pListener.Pending())
		{
			string newClientName = $"Client_{pClientIndex}";

			TcpClient newClient = pListener.AcceptTcpClient();
			_clients.Add(newClientName, newClient);

			//Log "joined session" message
			Console.WriteLine($"Accepted new client: {newClientName}");
			string welcomeMessage = $"You joined the room as {newClientName}!";
			NetworkStream stream = newClient.GetStream();
			StreamUtil.Write(stream, System.Text.Encoding.UTF8.GetBytes(welcomeMessage));
			
			//Increment ID for next user.
			pClientIndex++;

			foreach (KeyValuePair<string, TcpClient> client in _clients)
			{
				if (client.Key == newClientName)
					continue;

				string newClientMessage = $"{newClientName} has joined the server. Total online: {_clients.Count}";
				StreamUtil.Write(client.Value.GetStream(), System.Text.Encoding.UTF8.GetBytes(newClientMessage));
			}
		}	
	}

	private static void processExistingClients()
	{
		//Second big change, instead of blocking on one client, 
		//we now process all clients IF they have data available
		foreach (KeyValuePair<string, TcpClient> client in _clients)
		{
			if (client.Value.Available == 0) 
				continue;
			
			NetworkStream stream = client.Value.GetStream();

			//Log new input
			byte[] receivedData = StreamUtil.Read(stream);
			string input = System.Text.Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);
			if (isCommand(input))
			{
				sortCommand(input, client.Key);
				return;
			}

			echoMessageToAllClients(receivedData, client.Key);
		}	
	}

	private static bool isCommand(string pCommand) => pCommand.StartsWith("/");

	private static void sortCommand(string pCommand, string pClientName)
	{
		string removedSlash = pCommand.Substring(1);
		string lowerCase = removedSlash.ToLower();
		string[] seperatedElements = lowerCase.Split();
		string filteredCommnand = seperatedElements[0];

		switch (filteredCommnand)
		{
			case "setname":
				if (!checkIfNewNameIsValid(pClientName, seperatedElements[1]))
					return;
				
				setClientName(pClientName, seperatedElements[1]);
				break;
			case "list":
				//Log all connected clients
				break;
			case "help":
				//Information about all possible chat commands
				break;
		}
	}

	private static bool checkIfNewNameIsValid(string pFromUser, string pNewName)
	{
		string errorMessage = "";
		if (_clients.ContainsKey(pNewName))
		{
			errorMessage = $"{pNewName} is already taken. Please choose another.";
			sendServerMessageToUser(pFromUser, errorMessage);
			return false;
		}

		//Expand with other limitations if necessary (profanity filter, characters etc)
		return true;
	}
	
	private static void setClientName(string pClientName, string pNewName)
	{
		//can't change dictionary keys so have to remove and add it again with the new name.
		_clients.TryGetValue(pClientName, out TcpClient tempStoredClient);
		_clients.Remove(pClientName);

		string lowercaseName = pNewName.ToLower();
		_clients.Add(lowercaseName, tempStoredClient);

		string confirmationMessage = $"{pClientName} successfully changed their ID to '{lowercaseName}'";
		sendServerMessageToAllUsers(confirmationMessage);
	}

	private static void cleanupFaultyClients()
	{
		Dictionary<string, TcpClient> tempClients = _clients;

		foreach (KeyValuePair<string, TcpClient> client in tempClients)
		{
			if (client.Value.Connected)
				continue;
			
			tempClients.Remove(client.Key);
		}

		_clients = tempClients;
	}

	/// <summary>
	/// Retrieve and convert incoming bytes to string. Insert client's name before that data and send it back.
	/// </summary>
	/// <param name="pStream">NetworkStream from which we retrieve data</param>
	/// <param name="pClientName">e.g. "Client_01", used to insert before the message that user sent</param>
	/// <returns></returns>
	private static string concatenateNameInData(NetworkStream pStream, string pClientName)
	{
		byte[] receivedData = StreamUtil.Read(pStream);
		string textRepresentation = System.Text.Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);
		return pClientName + ": " + textRepresentation;
	}

	/// <summary>
	/// Loops over all clients connected to the server and writes to their network streams.
	/// </summary>
	/// <param name="pPublicMessage"></param>
	/// <param name="pClients"></param>
	private static void echoMessageToAllClients(byte[] pIncomingBuffer, string pFromUser)
	{
		string publicMessage = Encoding.UTF8.GetString(pIncomingBuffer);
		
		foreach(KeyValuePair<string, TcpClient> client in _clients)
		{
			string userName = pFromUser;
			
			if (pFromUser == client.Key)
				userName = "You";

			string data = $"{userName}: {publicMessage}";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
			NetworkStream stream = client.Value.GetStream();

			StreamUtil.Write(stream, buffer);
		}
	}

	private static void sendServerMessageToAllUsers(string pMessage)
	{
		//Bad implementation to convert double. TODO
		byte[] buffer = Encoding.UTF8.GetBytes(pMessage);
		
		echoMessageToAllClients(buffer, "Server");
	}

	private static void sendServerMessageToUser(string pClientName, string pMessage)
	{
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(pMessage);

		_clients.TryGetValue(pClientName, out TcpClient client);
		if (client == null)
			return;
		StreamUtil.Write(client.GetStream(), buffer);
	}
}


