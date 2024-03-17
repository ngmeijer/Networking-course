using System.Collections.Generic;
using System.Net.Sockets;

class CommandsHelper
{
	private static Dictionary<string, TcpClient> _clients;

	public delegate void SendPublicServerMessage(string pMessage);
	public delegate void SendPrivateServerMessage(string pClientName, string pMessage);

	public delegate void SendUserToUserMessage(string pFromClient, string pToClient, string pMessage);

	public event SendPublicServerMessage E_ServerPublicMessage;
	public event SendPrivateServerMessage E_ServerPrivateMessage;
	public event SendUserToUserMessage E_UserToUserMessage;
	
	public void SetClients(Dictionary<string, TcpClient> pClients)
	{
		_clients = pClients;
	}
	
	public bool isCommand(string pCommand) => pCommand.StartsWith("/");

	public void sortCommand(string pCommand, string pClientName = null)
	{
		string removedSlash = pCommand.Substring(1);
		string lowerCase = removedSlash.ToLower();
		string[] seperatedElements = lowerCase.Split();
		string filteredCommnand = seperatedElements[0];

		//Find a better way instead of switch case.
		switch (filteredCommnand)
		{
			case "setname":
				if (!checkIfNewNameIsValid(pClientName, seperatedElements[1]))
					return;
				
				setClientName(pClientName, seperatedElements[1]);
				break;
			case "list":
				//Log all connected clients
				listAllClients(pClientName);
				break;
			case "help":
				//Information about all possible chat commands. Find a better way for this.
				string helpMessage = "/setname <newID>" +
				                     "\n	-sets a new unique clientID." +
				                     "\n/whisper <userID> <message>" +
				                     "\n	Send a private message to specified user." +
				                     "\n/list" + 
				                     "\n	-lists all connected clients to the server." + 
				                     "\n/help" +
				                     "\n	-lists all available commands.";
				OnServerPrivateMessage(pClientName, helpMessage);
				break;
			case "whisper":
				string targetUser = seperatedElements[1];
				int index = lowerCase.IndexOf(targetUser);
				string message = lowerCase.Substring(index + targetUser.Length).Trim();
				OnUserToUserMessage(pClientName, targetUser, message);
				break;
			default:
				string errorMessage = $"'{pCommand}' is not a valid command.";
				OnServerPrivateMessage(pClientName, errorMessage);
				break;
		}
	}

	
	private bool checkIfNewNameIsValid(string pFromUser, string pNewName)
	{
		string errorMessage = "";
		if (_clients.ContainsKey(pNewName))
		{
			errorMessage = $"{pNewName} is already taken. Please choose another.";
			OnServerPrivateMessage(pFromUser, errorMessage);
			return false;
		}

		//Expand with other limitations if necessary (profanity filter, characters etc)
		return true;
	}
	
	private void setClientName(string pClientName, string pNewName)
	{
		//can't change dictionary keys so have to remove and add it again with the new name.
		_clients.TryGetValue(pClientName, out TcpClient tempStoredClient);
		_clients.Remove(pClientName);

		string lowercaseName = pNewName.ToLower();
		_clients.Add(lowercaseName, tempStoredClient);

		string confirmationMessage = $"{pClientName} successfully changed their ID to '{lowercaseName}'";
		OnServerPublicMessage(confirmationMessage);
	}

	private void listAllClients(string pClientName)
	{
		string namesMessage = "All clients connected to this server:";
		foreach (KeyValuePair<string, TcpClient> client in _clients)
		{
			namesMessage += $"\n	{client.Key}";
		}

		OnServerPrivateMessage(pClientName, namesMessage);
	}

	private void OnServerPrivateMessage(string pClientName, string pMessage)
	{
		E_ServerPrivateMessage?.Invoke(pClientName, pMessage);
	}

	private void OnServerPublicMessage(string pMessage)
	{
		E_ServerPublicMessage?.Invoke(pMessage);
	}

	private void OnUserToUserMessage(string pFromClient, string pToClient, string pMessage)
	{
		E_UserToUserMessage?.Invoke(pFromClient, pToClient, pMessage);
	}
}