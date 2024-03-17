﻿using System;
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

		CommandsHelper commandsHelper = new CommandsHelper();
		commandsHelper.SetClients(_clients);
		commandsHelper.E_ServerPrivateMessage += sendServerMessageToUser;
		commandsHelper.E_ServerPublicMessage += sendServerMessageToAllUsers;
		commandsHelper.E_UserToUserMessage += sendWhisperMessage;
		
		int clientIndex = 0;
		while (true)
		{
			processNewClients(listener, clientIndex);
			processExistingClients(commandsHelper);
			
			//Isn't it better to have this be the first call in the loop? Otherwise processExistingClients might run into disconnected clients.
			cleanupFaultyClients();

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
			string newClientName = $"client_{pClientIndex}";

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

	private static void processExistingClients(CommandsHelper pCommandsHelper)
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
			if (pCommandsHelper.isCommand(input))
			{
				pCommandsHelper.sortCommand(input, client.Key);
				return;
			}

			echoMessageToAllClients(receivedData, client.Key);
		}	
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

	private static void sendWhisperMessage(string pFromClient, string pToClient, string pMessage)
	{
		string messageToSender = $"You -> {pToClient}: {pMessage}";
		byte[] senderBuffer = System.Text.Encoding.UTF8.GetBytes(messageToSender);
		_clients.TryGetValue(pFromClient, out TcpClient sender);
		
		string messageToReceiver = $"(whisper) {pFromClient}: {pMessage}";
		byte[] receiverBuffer = System.Text.Encoding.UTF8.GetBytes(messageToReceiver);
		_clients.TryGetValue(pToClient, out TcpClient receiver);

		if (receiver == null)
		{
			string errorMessage = $"User '{pToClient}' does not exist.";
			byte[] errorBuffer = System.Text.Encoding.UTF8.GetBytes(errorMessage);
			StreamUtil.Write(sender.GetStream(), errorBuffer);
			return;
		}
		
		StreamUtil.Write(sender.GetStream(), senderBuffer);
		StreamUtil.Write(receiver.GetStream(), receiverBuffer);
	}
}