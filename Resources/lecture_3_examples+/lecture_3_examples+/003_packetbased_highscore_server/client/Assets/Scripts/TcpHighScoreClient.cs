using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

/**
 * Demonstrates a simple high score view using Packet based TCP communication
 */
public class TcpHighScoreClient : MonoBehaviour
{
    [SerializeField] private string _server = "localhost";
    [SerializeField] private int _port = 55555;

    [SerializeField] private HighScoreView _highScoreView = null;

    private TcpClient _client;

    private Score _lastAddedPlayerScore = null;
    private List<Score> _scores = new List<Score>();

    void Start()
    {
        _highScoreView.OnScoreAdded += onScoreAdded;
        _highScoreView.OnScoresRequested += onScoresRequested;

        connectToServer();
    }

    private void connectToServer()
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(_server, _port);
            Debug.Log ("Connected to server.");
        }
        catch (Exception e)
        {
            _highScoreView.SetPlayerScoreHeader("Not connected to server.");
            Debug.Log(e.Message);
        }
    }

    private void onScoreAdded(Score pScore)
    {
        //store the last score the player wants to add locally
        _lastAddedPlayerScore = pScore;

        //send the add score command
        Packet outPacket = new Packet();
        outPacket.Write("addscore");
        outPacket.Write(pScore.name);
        outPacket.Write(pScore.score);
        sendPacket(outPacket);
    }

    private void onScoresRequested()
    {
        //send the getscore command
        Packet outPacket = new Packet();
        outPacket.Write("getscores");
        sendPacket(outPacket);
    }

    private void sendPacket (Packet pOutPacket)
    {
        try {
            Debug.Log("Sending:" + pOutPacket);
            StreamUtil.Write(_client.GetStream(), pOutPacket.GetBytes());
        }

        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            _client.Close();
            connectToServer();
        }
    }

    private void Update()
    {
        try
        {
            if (_client.Available > 0)
            {
                Packet inPacket = new Packet(StreamUtil.Read(_client.GetStream()));
                string command = inPacket.ReadString();
                if (command == "highscores") handleHighScores(inPacket);
            }
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            _client.Close();
            connectToServer();
        }
    }


    /**
     * Note the difference, the first element from the packet has already 
     * been taken out of the packet, when it gets here!
     */
    private void handleHighScores(Packet pInPacket)
    {
        _scores.Clear();

        int highScoreCount = pInPacket.ReadInt();

        for (int i = 0; i < highScoreCount; i++)
        {
            _scores.Add(new Score(pInPacket.ReadString(), pInPacket.ReadInt()));
        }

        //same as previous
        _scores.Sort((b, a) => a.score.CompareTo(b.score));

        //do we have the highscore? (this assumes unique playernames etc)
        bool highScore =
            (highScoreCount > 0) &&
            (_lastAddedPlayerScore != null) &&
            (_scores[0].name == _lastAddedPlayerScore.name && _scores[0].score == _lastAddedPlayerScore.score);

        _highScoreView.SetPlayerScoreHeader(highScore ? "!!! NEW HIGHSCORE !!!" : "YOUR SCORE");
        _highScoreView.SetPlayerScore(_lastAddedPlayerScore);
        _highScoreView.SetHighScores(_scores);
    }
}

