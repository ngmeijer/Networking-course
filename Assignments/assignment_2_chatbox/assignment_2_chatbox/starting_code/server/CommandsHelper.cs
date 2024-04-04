using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Windows.Input;

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

		switch (filteredCommnand)
		{
			case "setname":
				handleSetClientNameCommand(pClientName, seperatedElements[1]);
				break;
			case "list":
				handleListClientsCommand(pClientName);
				break;
			case "help":
				handleHelpCommand(pClientName);
				break;
			case "whisper":
				handleWhisperCommand(seperatedElements, lowerCase, pClientName);
				break;
			default:
				handleInvalidCommand(pCommand, pClientName);
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

		return true;
	}

	private void handleInvalidCommand(string pCommand, string pFromClient)
	{
        string errorMessage = $"'{pCommand}' is not a valid command.";
        OnServerPrivateMessage(pFromClient, errorMessage);
    }
	
	private void handleSetClientNameCommand(string pClientName, string pNewName)
	{
        if (!checkIfNewNameIsValid(pClientName, pNewName))
            return;

        //can't change dictionary keys so have to remove and add it again with the new name.
        _clients.TryGetValue(pClientName, out TcpClient tempStoredClient);
		_clients.Remove(pClientName);

		string lowercaseName = pNewName.ToLower();
		_clients.Add(lowercaseName, tempStoredClient);

		string confirmationMessage = $"{pClientName} successfully changed their ID to '{lowercaseName}'";
		OnServerPublicMessage(confirmationMessage);
	}

	private void handleListClientsCommand(string pClientName)
	{
		string namesMessage = "All clients connected to this server:";
		foreach (KeyValuePair<string, TcpClient> client in _clients)
		{
			namesMessage += $"\n	{client.Key}";
		}

		OnServerPrivateMessage(pClientName, namesMessage);
	}

	private void handleWhisperCommand(string[] pSeperatedElements, string pLowerCaseData, string pFromClient)
	{
        if (pSeperatedElements.Length == 1)
        {
            string errorMessage = "A client name to deliver the message to must be given.";
            OnServerPrivateMessage(pFromClient, errorMessage);
            return;
        }
        string targetUser = pSeperatedElements[1];
        int index = pLowerCaseData.IndexOf(targetUser);
        string message = pLowerCaseData.Substring(index + targetUser.Length).Trim();
        OnUserToUserMessage(pFromClient, targetUser, message);
    }

    private void handleHelpCommand(string pClientName)
	{
        string message = "/setname <newID>" +
                                     "\n	-sets a new unique clientID." +
                                     "\n/whisper <userID> <message>" +
                                     "\n	Send a private message to specified user." +
                                     "\n/list" +
                                     "\n	-lists all connected clients to the server." +
                                     "\n/help" +
                                     "\n	-lists all available commands.";
        OnServerPrivateMessage(pClientName, message);
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