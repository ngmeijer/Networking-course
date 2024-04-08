using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using shared;
using System.Threading;
using System.Diagnostics;
using server;
using System.Linq;

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
            checkFaultyClients();

            Thread.Sleep(100);
        }
    }

    private void processNewClients()
    {
        while (_listener.Pending())
        {
            TcpClient client = _listener.AcceptTcpClient();
            _clientAvatars.Add(client, null);
            AvatarContainer newAvatar = new AvatarContainer()
            {
                ID = _clientAvatars.Count - 1,
                SkinID = randomNumberGenerator.Next(0, 100),
            };
            _clientAvatars[client] = newAvatar;

            Console.WriteLine($"Accepted new client - {newAvatar.ID} -");
            //Let the newly connected client know it should add an avatar.
            //After this, the server expects the AvatarContainer back but with a Position assigned (I made the choice to do position generation on the client side, because designers might choose to use some kind of tooling to determine where the avatar should spawn. Or, if in the future a decision is made to use NavMesh, it needs to happen on the client side anyway).
            //Then, all other clients will be notified of the new avatar including the random position.
            _requestHandler.SendNewAvatar(client, newAvatar);
        }
    }

    private void processExistingClients()
    {
        //Loop over every client and check if they have sent any data to server.
        foreach (KeyValuePair<TcpClient, AvatarContainer> client in _clientAvatars)
        {
            if (client.Key.Available == 0) 
                continue;

            ISerializable inObject = readIncomingData(client.Key);
            Console.WriteLine($"Received object type: {inObject}");
            switch (inObject)
            {
                //Distribute avatar reps (only happens when a new client joins) to all clients
                case AvatarContainer avatar:
                    syncNewAvatarsAcrossClients(client.Key, avatar);
                    break;
                //Distribute messages to all clients
                case SimpleMessage message:
                    syncMessagesAcrossClients(message);
                    break;
                //Distribute position update to all clients.
                case PositionUpdate positionReq:
                    syncPositionsAcrossClients(client.Key, positionReq);
                    break;
            }
        }
    }

    private void syncNewAvatarsAcrossClients(TcpClient pNewClient, AvatarContainer pClientReturnedAvatar)
    {
        if (_clientAvatars.TryGetValue(pNewClient, out AvatarContainer storedAvatar))
        {
            storedAvatar.Position = pClientReturnedAvatar.Position;
        }

        List<TcpClient> clients = _clientAvatars.Keys.ToList();
        Console.WriteLine($"Received random position for new client. Total client count: {clients.Count}");
        //Other clients must be notified of new client's avatar.
        for(int clientIndex = 0; clientIndex < clients.Count; clientIndex++)
        {
            TcpClient currentClient = clients[clientIndex];

            //If the current client in the list IS the new client, send the avatars of existing clients to the new client as well.
            if (currentClient == pNewClient)
            {
                //Loop over clients/avatars.
                foreach(KeyValuePair<TcpClient, AvatarContainer> pair in _clientAvatars)
                {
                    //If the loop hits the new client itself, skip it. Otherwise client side throws error that an avatar already exists (+ unnecessary traffic)
                    if (pair.Key == pNewClient)
                        continue;

                    _requestHandler.SendNewAvatar(pNewClient, pair.Value);
                }
                continue;
            }

            Console.WriteLine($"Notifying client {clientIndex} of new avatar {pClientReturnedAvatar.ID}");
            _requestHandler.SendNewAvatar(currentClient, pClientReturnedAvatar);
        }
    }

    private void syncPositionsAcrossClients(TcpClient pClient, PositionUpdate pPositionUpdate)
    {
        _clientAvatars[pClient].Position = pPositionUpdate.Position;

        foreach (KeyValuePair<TcpClient, AvatarContainer> client in _clientAvatars)
        {
            Console.WriteLine($"Moving avatar {pPositionUpdate.ID} for client {client.Value.ID} to position ({pPositionUpdate.Position[0]},{pPositionUpdate.Position[1]}, {pPositionUpdate.Position[2]})");
            _requestHandler.SendPositionUpdate(client.Key, pPositionUpdate);
        }
    }

    private void syncMessagesAcrossClients(SimpleMessage pMessage)
    {
        foreach (KeyValuePair<TcpClient, AvatarContainer> client in _clientAvatars)
        {
            _requestHandler.SendMessage(client.Key, pMessage);
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

    }
}