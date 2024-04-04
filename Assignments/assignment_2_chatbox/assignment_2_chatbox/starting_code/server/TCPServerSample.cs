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

    public static void Main(string[] args)
    {
        Console.WriteLine("Server started on port 55555");

        TcpListener listener = new TcpListener(IPAddress.Any, 55555);
        listener.Start();

        CommandsHelper commandsHelper = new CommandsHelper();
        commandsHelper.SetClients(_clients);
        commandsHelper.E_ServerPrivateMessage += sendMessageToUser;
        commandsHelper.E_ServerPublicMessage += sendServerMessageToAllUsers;
        commandsHelper.E_UserToUserMessage += sendWhisperMessage;

        int clientIndex = 0;
        int timeIndex = 0;
        while (true)
        {
            Console.WriteLine($"Server running - {timeIndex}");
            timeIndex += 1;
            processNewClients(listener, clientIndex);
            processExistingClients(commandsHelper);
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
            try
            {
                string newClientName = $"client_{pClientIndex}";

                TcpClient newClient = pListener.AcceptTcpClient();
                _clients.Add(newClientName, newClient);

                Console.WriteLine($"Accepted new client: {newClientName}");
                string welcomeMessage = $"You joined the room as {newClientName}!";
                sendMessageToUser(newClientName, welcomeMessage);

                pClientIndex++;

                foreach (KeyValuePair<string, TcpClient> client in _clients)
                {
                    if (client.Key == newClientName)
                        continue;

                    string newClientMessage = $"{newClientName} has joined the server. Total online: {_clients.Count}";
                    sendMessageToUser(client.Key, newClientMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private static void processExistingClients(CommandsHelper pCommandsHelper)
    {
        foreach (KeyValuePair<string, TcpClient> client in _clients)
        {
            try
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


                echoMessageToAllClients(input, client.Key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private static void cleanupFaultyClients()
    {
        Dictionary<string, TcpClient> removedClients = new Dictionary<string, TcpClient>();

        foreach (KeyValuePair<string, TcpClient> client in _clients)
        {
            try
            {
                string testMessage = "Server ping";
                sendMessageToUser(client.Key, testMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                removedClients.Add(client.Key, client.Value);
            }
        }

        foreach (KeyValuePair<string, TcpClient> client in removedClients)
        {
            _clients.Remove(client.Key);
        }
    }

    /// <summary>
    /// Loops over all clients connected to the server and writes to their network streams.
    /// </summary>
    /// <param name="pPublicMessage"></param>
    /// <param name="pClients"></param>
    private static void echoMessageToAllClients(string pMessage, string pFromUser)
    {
        foreach (KeyValuePair<string, TcpClient> client in _clients)
        {
            string userName = pFromUser;

            if (pFromUser == client.Key)
                userName = "You";

            string data = $"{userName}: {pMessage}";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
            NetworkStream stream = client.Value.GetStream();

            StreamUtil.Write(stream, buffer);
        }
    }

    private static void sendServerMessageToAllUsers(string pMessage)
    {
        echoMessageToAllClients(pMessage, "Server");
    }

    private static void sendMessageToUser(string pClientName, string pMessage)
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
        _clients.TryGetValue(pFromClient, out TcpClient sender);

        string messageToReceiver = $"(whisper) {pFromClient}: {pMessage}";
        _clients.TryGetValue(pToClient, out TcpClient receiver);

        if (receiver == null)
        {
            string errorMessage = $"User ' {pToClient} ' does not exist.";
            sendMessageToUser(pFromClient, errorMessage);
            return;
        }

        sendMessageToUser(pFromClient, messageToSender);
        sendMessageToUser(pToClient, messageToReceiver);
    }
}