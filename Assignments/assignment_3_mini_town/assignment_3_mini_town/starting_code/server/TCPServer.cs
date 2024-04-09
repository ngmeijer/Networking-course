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
using System.Runtime.Remoting.Messaging;
using shared.src.protocol;
using shared.src;

/**
 * This class implements a simple tcp echo server.
 * Read carefully through the comments below.
 * Note that the server does not contain any sort of error handling.
 */
class TCPServer
{
    private Random randomNumberGenerator = new Random();
    private RequestHandler _requestHandler = new RequestHandler();
    private List<TcpClient> _connectedClients = new List<TcpClient>();

    private const float HEARTBEAT_INTERVAL = 2f;

    public static void Main(string[] args)
    {
        TCPServer server = new TCPServer();
        server.run();
    }

    private TcpListener _listener;
    private Dictionary<TcpClient, NewAvatar> _clientAvatars = new Dictionary<TcpClient, NewAvatar>();

    private void run()
    {
        Console.WriteLine("Server started on port 55555");

        _listener = new TcpListener(IPAddress.Any, 55555);
        _listener.Start();

        while (true)
        {
            //checkFaultyClients();
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
            NewAvatar newAvatar = new NewAvatar()
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
        foreach (KeyValuePair<TcpClient, NewAvatar> incomingDataFromClient in _clientAvatars)
        {
            if (incomingDataFromClient.Key.Available == 0)
                continue;

            ISerializable inObject = readIncomingData(incomingDataFromClient.Key);
            Console.WriteLine($"Received object type: {inObject}");
            switch (inObject)
            {
                //Distribute avatar reps (only happens when a new client joins) to all clients
                case NewAvatar avatar:
                    syncNewAvatarsAcrossClients(incomingDataFromClient.Key, avatar);
                    break;
                //Distribute messages to all clients
                case SimpleMessage message:
                    syncMessagesAcrossClients(incomingDataFromClient.Key, message);
                    break;
                //Distribute position update to all clients.
                case PositionUpdate positionReq:
                    syncPositionsAcrossClients(incomingDataFromClient.Key, positionReq);
                    break;
                case HeartBeat heartBeat:
                    _connectedClients.Add(incomingDataFromClient.Key);
                    break;
            }
        }
    }

    private void checkFaultyClients()
    {
        _connectedClients.Clear();

        Dictionary<TcpClient, NewAvatar> disconnectedClients = new Dictionary<TcpClient, NewAvatar>();
        foreach (KeyValuePair<TcpClient, NewAvatar> heartbeatCheck in _clientAvatars)
        {
            _requestHandler.SendHeartBeat(heartbeatCheck.Key, new HeartBeat() { ClientID = heartbeatCheck.Value.ID });
        }

        foreach (KeyValuePair<TcpClient, NewAvatar> disconnectedClient in disconnectedClients)
        {
            _clientAvatars.Remove(disconnectedClient.Key);
        }

        foreach (KeyValuePair<TcpClient, NewAvatar> heartbeatCheck in _clientAvatars)
        {
            _requestHandler.SendAvatarRemove(heartbeatCheck.Key, new DeadAvatar() { ID = heartbeatCheck.Value.ID });
        }
    }

    private void syncNewAvatarsAcrossClients(TcpClient pNewClient, NewAvatar pClientReturnedAvatar)
    {
        //Update the position received from the client
        _clientAvatars.TryGetValue(pNewClient, out NewAvatar storedNewAvatar);
        storedNewAvatar.Position = pClientReturnedAvatar.Position;

        if (_clientAvatars.Count > 1)
        {
            //Send all avatars to new client
            ExistingAvatars existingAvatarsContainer = new ExistingAvatars()
            {
                AvatarCount = _clientAvatars.Count,
                Avatars = new NewAvatar[_clientAvatars.Count]
            };

            NewAvatar[] avatarArray = _clientAvatars.Values.ToArray();
            for(int i = 0; i < _clientAvatars.Count; i++)
            {
                NewAvatar currentAvatar = avatarArray[i];
                existingAvatarsContainer.Avatars[i] = new NewAvatar()
                {
                    ID = currentAvatar.ID,
                    SkinID = currentAvatar.SkinID,
                    Position = currentAvatar.Position,
                };
            }
            
            //for(int i = 0; i < tempAvatars.Length; i++)
            //{
            //    existingAvatarsContainer.AvatarIDs[i] = tempAvatars[i].ID;
            //}

            _requestHandler.SendExistingClients(pNewClient, existingAvatarsContainer);
        }

        ////Update existing clients about new avatar.
        //foreach(KeyValuePair<TcpClient, NewAvatar> pair in _clientAvatars)
        //{
        //    _requestHandler.SendNewAvatar(pair.Key, storedNewAvatar);
        //}
    }

    private void syncPositionsAcrossClients(TcpClient pClient, PositionUpdate pPositionUpdate)
    {
        _clientAvatars[pClient].Position = pPositionUpdate.Position;

        foreach (KeyValuePair<TcpClient, NewAvatar> client in _clientAvatars)
        {
            Console.WriteLine($"Moving avatar {pPositionUpdate.ID} for client {client.Value.ID} to position ({pPositionUpdate.Position.ToString()})");
            _requestHandler.SendPositionUpdate(client.Key, pPositionUpdate);
        }
    }

    private void syncMessagesAcrossClients(TcpClient pSender, SimpleMessage pMessage)
    {
        foreach (KeyValuePair<TcpClient, NewAvatar> receiver in _clientAvatars)
        {
            //If it is not a whisper message, send it to all avatars.
            if (!isWhisperMessage(pMessage))
            {
                _requestHandler.SendMessage(receiver.Key, pMessage);
                continue;
            }

            //Take out the /whisper command
            pMessage = filterMessage(pMessage);

            //
            if (receiver.Key != pSender && isReceiverInRange(2, pMessage.Position, receiver.Value.Position))
            {
                _requestHandler.SendMessage(receiver.Key, pMessage);
            }
        }
    }

    private SimpleMessage filterMessage(SimpleMessage pMessage)
    {
        string text = pMessage.Text;
        string command = "/whisper";
        int index = text.IndexOf(command);
        string filteredMessage = pMessage.Text.Substring(index + command.Length).Trim();
        pMessage.Text = filteredMessage;

        return pMessage;
    }

    private bool isWhisperMessage(SimpleMessage pMessage)
    {
        string[] data = pMessage.Text.Split();
        if (data[0] == "/whisper")
            return true;

        return false;
    }

    private bool isReceiverInRange(float pMaxDistance, Vector3 pAvatarSenderPosition, Vector3 pAvatarReceiverPosition)
    {
        double xDifference = pAvatarSenderPosition.x - pAvatarReceiverPosition.x;
        double yDifference = pAvatarSenderPosition.y - pAvatarReceiverPosition.y;
        double zDifference = pAvatarSenderPosition.z - pAvatarReceiverPosition.z;

        double distance = Math.Sqrt(Math.Pow(xDifference, 2) + Math.Pow(yDifference, 2) + Math.Pow(zDifference, 2));
        Console.WriteLine($"Distance: {distance}");
        if (distance <= pMaxDistance)
            return true;

        return false;
    }

    private ISerializable readIncomingData(TcpClient pSender)
    {
        byte[] inBytes = StreamUtil.Read(pSender.GetStream());
        Packet inPacket = new Packet(inBytes);
        return inPacket.ReadObject();
    }
}