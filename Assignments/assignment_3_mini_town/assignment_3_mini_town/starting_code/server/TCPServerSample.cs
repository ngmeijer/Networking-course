using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using shared;
using System.Threading;
using System.Diagnostics;

/**
 * This class implements a simple tcp echo server.
 * Read carefully through the comments below.
 * Note that the server does not contain any sort of error handling.
 */
class TCPServerSample
{
	private Random randomNumberGenerator = new Random();

	public static void Main(string[] args)
	{
		TCPServerSample server = new TCPServerSample();
		server.run();
	}

	private TcpListener _listener;
	private List<TcpClient> _clients = new List<TcpClient>();
	private List<AvatarContainer> _avatars = new List<AvatarContainer>();

	private void run()
	{
		Console.WriteLine("Server started on port 55555");

		_listener = new TcpListener(IPAddress.Any, 55555);
		_listener.Start();

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
			TcpClient client = _listener.AcceptTcpClient();
			_clients.Add(client);

			createNewAvatar(client);
            synchronizeAvatars();
		}
	}

    private void synchronizeAvatars()
    {
        foreach (TcpClient currentClient in _clients)
        {
            foreach (AvatarContainer avatar in _avatars)
            {
                sendObject(currentClient, avatar);
            }
        }
    }

	private void createNewAvatar(TcpClient pClient)
	{
        int id = _clients.Count - 1;
        int skinID = randomNumberGenerator.Next(0, 1000);

        float[] position = getRandomValidPosition(pClient);
        AvatarContainer newAvatar = new AvatarContainer()
        {
            ID = id,
            SkinID = skinID,
            Position = position,
        };


        _avatars.Add(newAvatar);

        Console.WriteLine($"Accepted client: {newAvatar.ID}.");
    }

    private void processExistingClients()
	{
		checkFaultyClients();

        foreach (TcpClient sender in _clients)
		{
			if (sender.Available == 0) continue;

            ISerializable inObject = readIncomingData(sender);
            if(inObject is AvatarContainer avatar)
            {
                int index = _avatars.IndexOf(avatar);
            }
			loopDataToClients(inObject); 
        }
	}

    private float[] getRandomValidPosition(TcpClient pClient, AvatarContainer pContainer)
    {
        sendObject(pClient, pContainer);

        return position;
    }

    private ISerializable readIncomingData(TcpClient pSender)
    {
        byte[] inBytes = StreamUtil.Read(pSender.GetStream());
        Packet inPacket = new Packet(inBytes);
        return inPacket.ReadObject();
    }

	private void checkFaultyClients()
	{
        List<TcpClient> tempClients = _clients;

        foreach (TcpClient client in tempClients)
        {
            if (client.Connected)
                continue;

            tempClients.Remove(client);
            Console.WriteLine("removed faulty client.");
        }

        _clients = tempClients;
    }

    private void loopDataToClients(ISerializable pObject)
    {
		foreach(TcpClient client in _clients)
		{
			sendObject(client, pObject);
		}
    }

    private void sendObject(TcpClient pClient, ISerializable pOutObject)
    {
        try
        {
            Console.WriteLine("Sending to all clients: " + pOutObject);

            Packet outPacket = new Packet();
            outPacket.Write(pOutObject);

            StreamUtil.Write(pClient.GetStream(), outPacket.GetBytes());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}