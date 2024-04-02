using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

/**
 * Demonstrates a simple high score view using an object based communication approach
 */
public class TcpHighScoreClient : MonoBehaviour
{
    [SerializeField] private string _server = "localhost";
    [SerializeField] private int _port = 55555;

    [SerializeField] private HighScoreView _highScoreView = null;

    private TcpClient _client;

    private List<Score> _scores = new List<Score>();
    private Score _lastAddedPlayerScore = null;

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
        AddRequest addRequest = new AddRequest();
        addRequest.score = pScore;
        sendObject(addRequest);
    }

    private void onScoresRequested()
    {
        //send the getscore command
        sendObject(new GetRequest());
    }

    private void sendObject (ISerializable pOutObject)
    {
        try {
            Debug.Log("Sending:" + pOutObject);

            Packet outPacket = new Packet();
            outPacket.Write(pOutObject);

            StreamUtil.Write(_client.GetStream(), outPacket.GetBytes());
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
                byte[] inBytes = StreamUtil.Read(_client.GetStream());
                Packet inPacket = new Packet(inBytes);
                ISerializable inObject = inPacket.ReadObject();

                if (inObject is HighscoresUpdate)   {   handleHighScores(inObject as HighscoresUpdate);    }
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

    private void handleHighScores(HighscoresUpdate pHighscoresUpdate)
    {
        //new
        _scores = pHighscoresUpdate.scores;

        //same as previous
        _scores.Sort((b, a) => a.score.CompareTo(b.score));

        //do we have the highscore? (this assumes unique playernames etc)
        bool highScore =
            (_scores.Count > 0) &&
            (_lastAddedPlayerScore != null) &&
            (_scores[0].name == _lastAddedPlayerScore.name && _scores[0].score == _lastAddedPlayerScore.score);

        _highScoreView.SetPlayerScoreHeader(highScore ? "!!! NEW HIGHSCORE !!!" : "YOUR SCORE");
        _highScoreView.SetPlayerScore(_lastAddedPlayerScore);
        _highScoreView.SetHighScores(_scores);
    }
}

