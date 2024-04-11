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
    private DataSender _dataSender = new DataSender();
    private List<TcpClient> _connectedClients = new List<TcpClient>();
    private DataProcessor _dataProcessor;

    private const float HEARTBEAT_INTERVAL = 10f;
    private float _currentHeartbeat;

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

        _dataProcessor = new DataProcessor(_dataSender);
        _listener = new TcpListener(IPAddress.Any, 55555);
        _listener.Start();

        while (true)
        {
            checkFaultyClients();
            processNewClients();
            processExistingClients();

            Thread.Sleep(100);
        }
    }

    private void checkFaultyClients()
    {
        try
        {
            if (_currentHeartbeat >= HEARTBEAT_INTERVAL)
            {
                Dictionary<TcpClient, NewAvatar> disconnectedClients = new Dictionary<TcpClient, NewAvatar>();
                foreach (KeyValuePair<TcpClient, NewAvatar> pair in _clientAvatars)
                {
                    if (_connectedClients.Contains(pair.Key))
                        continue;

                    disconnectedClients.Add(pair.Key, pair.Value);
                }
                _connectedClients.Clear();

                foreach (KeyValuePair<TcpClient, NewAvatar> pair in disconnectedClients)
                {
                    _clientAvatars.Remove(pair.Key);
                }

                foreach (KeyValuePair<TcpClient, NewAvatar> remainingClient in _clientAvatars)
                {
                    foreach (KeyValuePair<TcpClient, NewAvatar> disconnectedClient in disconnectedClients)
                    {
                        _dataSender.SendAvatarRemove(remainingClient.Key, new DeadAvatar()
                        {
                            ID = disconnectedClient.Value.ID
                        });
                    }
                }

                foreach (KeyValuePair<TcpClient, NewAvatar> pair in _clientAvatars)
                {
                    _dataSender.SendHeartBeat(pair.Key, new HeartBeat());
                }
                _currentHeartbeat = 0;
            }
            _currentHeartbeat += 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private void processNewClients()
    {
        try
        {
            while (_listener.Pending())
            {
                TcpClient client = _listener.AcceptTcpClient();
                _clientAvatars.Add(client, null);
                NewAvatar newAvatar = new NewAvatar()
                {
                    ID = _clientAvatars.Count - 1,
                    SkinID = randomNumberGenerator.Next(0, 100),
                    Position = new Vector3().Zero(),
                };
                _clientAvatars[client] = newAvatar;

                Console.WriteLine($"Accepted new client - {newAvatar.ID} -");
                _dataSender.SendNewAvatar(client, newAvatar);
                _connectedClients.Add(client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void processExistingClients()
    {
        //Loop over every client and check if they have sent any data to server.
        foreach (KeyValuePair<TcpClient, NewAvatar> incomingDataFromClient in _clientAvatars)
        {
            if (incomingDataFromClient.Key.Available == 0)
                continue;
            try
            {
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
                        _dataProcessor.SyncMessagesAcrossClients(incomingDataFromClient.Key, message, _clientAvatars);
                        break;
                    case WhisperMessage message:
                        _dataProcessor.SyncWhisperMessagesAcrossClients(incomingDataFromClient.Key, message, _clientAvatars);
                        break;
                    //Distribute position update to all clients.
                    case PositionUpdate positionReq:
                        syncPositionsAcrossClients(incomingDataFromClient.Key, positionReq);
                        break;
                    case HeartBeat heartBeat:
                        Console.WriteLine("Registered new avatar. Distributing among clients.");
                        if (!_connectedClients.Contains(incomingDataFromClient.Key))
                        {
                            _connectedClients.Add(incomingDataFromClient.Key);
                        }
                        break;
                    case SkinUpdate skinUpdate:
                        syncSkinUpdateAcrossClients(incomingDataFromClient.Key, skinUpdate);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    private void syncSkinUpdateAcrossClients(TcpClient pClient, SkinUpdate pCurrentData)
    {
        Console.WriteLine("Received new skin request. Updating clients.");
        int randomSkinID = randomNumberGenerator.Next(0, 100);
        while (randomSkinID == pCurrentData.SkinID)
        {
            randomSkinID = randomNumberGenerator.Next(0, 100);
        }

        _clientAvatars.TryGetValue(pClient, out NewAvatar avatar);
        avatar.SkinID = randomSkinID;

        foreach (KeyValuePair<TcpClient, NewAvatar> pair in _clientAvatars)
        {
            _dataSender.SendSkinUpdate(pair.Key, new SkinUpdate()
            {
                ID = avatar.ID,
                SkinID = randomSkinID
            });
        }
    }

    private void syncNewAvatarsAcrossClients(TcpClient pNewClient, NewAvatar pClientReturnedAvatar)
    {
        Console.WriteLine("Registered new avatar. Distributing among clients.");
        //Update the position received from the client
        _clientAvatars.TryGetValue(pNewClient, out NewAvatar storedNewAvatar);
        storedNewAvatar.Position = pClientReturnedAvatar.Position;

        if (_clientAvatars.Count > 1)
        {
            ExistingAvatars existingAvatarsContainer = new ExistingAvatars()
            {
                AvatarCount = _clientAvatars.Count,
                Avatars = new NewAvatar[_clientAvatars.Count]
            };

            //Update new client
            NewAvatar[] avatarArray = _clientAvatars.Values.ToArray();
            for (int i = 0; i < _clientAvatars.Count; i++)
            {
                NewAvatar currentAvatar = avatarArray[i];
                existingAvatarsContainer.Avatars[i] = new NewAvatar()
                {
                    ID = currentAvatar.ID,
                    SkinID = currentAvatar.SkinID,
                    Position = currentAvatar.Position,
                };
            }

            //bit different approach here than what the requirements say:
            //1 large packet for the new client (with all the avatars including its own
            Packet newClientPacket = new Packet();
            newClientPacket.Write(existingAvatarsContainer);
            _dataSender.SendPacket(pNewClient, newClientPacket);

            //Update existing clients. X amount of very small packets with just the new client. Prevents unnecessary data sending (like if there are 1000 clients connected that will be a lot of data if we send 1 packet (with allll the avatars) rather than just the one that is necessary.
            TcpClient[] clientArray = _clientAvatars.Keys.ToArray();
            Packet existingClientsPacket = new Packet();
            existingClientsPacket.Write(storedNewAvatar);
            for (int i = 0; i < clientArray.Length; i++)
            {
                if (clientArray[i] == pNewClient)
                    continue;

                _dataSender.SendPacket(clientArray[i], existingClientsPacket);
            }
        }
    }

    private void syncPositionsAcrossClients(TcpClient pClient, PositionUpdate pPositionUpdate)
    {
        Console.WriteLine($"Avatar {pPositionUpdate.ID} has moved to position {pPositionUpdate.Position}.");
        _clientAvatars[pClient].Position = pPositionUpdate.Position;

        foreach (KeyValuePair<TcpClient, NewAvatar> client in _clientAvatars)
        {
            Console.WriteLine($"Moving avatar {pPositionUpdate.ID} for client {client.Value.ID} to position ({pPositionUpdate.Position.ToString()})");
            _dataSender.SendPositionUpdate(client.Key, pPositionUpdate);
        }
    }

    private ISerializable readIncomingData(TcpClient pSender)
    {
        byte[] inBytes = StreamUtil.Read(pSender.GetStream());
        Packet inPacket = new Packet(inBytes);
        return inPacket.ReadObject();
    }
}