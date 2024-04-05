using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using shared;
using System.Threading;
using System.Diagnostics;
using server;

/**
 * This class implements a simple tcp echo server.
 * Read carefully through the comments below.
 * Note that the server does not contain any sort of error handling.
 */
class TCPServer
{
    private Random randomNumberGenerator = new Random();
    private RequestHandler _requestHandler = new RequestHandler();

    public static void Main(string[] args)
    {
        TCPServer server = new TCPServer();
        server.run();
    }

    private TcpListener _listener;
    private Dictionary<TcpClient, AvatarContainer> _clientAvatars = new Dictionary<TcpClient, AvatarContainer>();

    private void run()
    {
        Console.WriteLine("Server started on port 55555");

        _listener = new TcpListener(IPAddress.Any, 55555);
        _listener.Start();

        while (true)
        {
            processNewClients();
            processExistingClients();

            Thread.Sleep(100);
        }
    }

    private void processNewClients()
    {
        while (_listener.Pending())
        {
            TcpClient client = _listener.AcceptTcpClient();
            _clientAvatars.Add(client, null);
            AvatarContainer newAvatar = _requestHandler.HandleNewAvatarCreation(client, _clientAvatars.Count - 1);
            _clientAvatars[client] = newAvatar;

            synchronizeAvatars();
        }
    }

    private void synchronizeAvatars()
    {
        foreach(KeyValuePair<TcpClient, AvatarContainer> client in _clientAvatars)
        {
            
        }
    }

    private void processExistingClients()
    {
        checkFaultyClients();

        foreach (KeyValuePair<TcpClient, AvatarContainer> client in _clientAvatars)
        {
            if (client.Key.Available == 0) continue;

            ISerializable inObject = readIncomingData(client.Key);
            switch (inObject)
            {
                case SimpleMessage message:
                    syncMessagesAcrossClients(message.Text);
                    break;
                case AvatarContainer avatar:
                    //int index = _clientAvatars.IndexOf(avatar);
                    break;
                case PositionRequest positionReq:
                    _clientAvatars[client.Key].Position = positionReq.Position;
                    break;
            }

            Console.WriteLine($"ID: {client.Value.ID}. \nSkinID: {client.Value.SkinID}. " +
                $"\nPosition: ({client.Value.Position[0]}, {client.Value.Position[1]}, {client.Value.Position[2]})");
        }
    }

    private void syncMessagesAcrossClients(string pMessage)
    {
        foreach(KeyValuePair<TcpClient, AvatarContainer> client in _clientAvatars)
        {
            _requestHandler.HandleMessageRequest(client.Key, client.Value.ID, pMessage);
        }
    }

    private ISerializable readIncomingData(TcpClient pSender)
    {
        byte[] inBytes = StreamUtil.Read(pSender.GetStream());
        Packet inPacket = new Packet(inBytes);
        return inPacket.ReadObject();
    }

    private void checkFaultyClients()
    {
        Dictionary<TcpClient, AvatarContainer> tempClients = _clientAvatars;

        foreach (KeyValuePair<TcpClient, AvatarContainer> client in _clientAvatars)
        {
            if (client.Key.Connected)
                continue;

            tempClients.Remove(client.Key);
            Console.WriteLine("removed faulty client.");
        }

        _clientAvatars = tempClients;
    }
}