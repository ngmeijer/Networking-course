using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/**
 * Demonstrates a simple high score view using string based tcp communication
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

    //  SENDING CODE

    private void onScoreAdded(Score pScore)
    {
        //store the last score the player wants to add locally
        _lastAddedPlayerScore = pScore;
        sendString("addscore," + pScore.name + "," + pScore.score);
		
        //try this out to crash the server ;):
		//sendString("addscore");
    }

    private void onScoresRequested()
    {
        sendString("getscores");
    }

    private void sendString(string pOutString)
    {
        try
        {
            Debug.Log("Sending:" + pOutString);
            byte[] outBytes = Encoding.UTF8.GetBytes(pOutString);
            StreamUtil.Write(_client.GetStream(), outBytes);
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            _client.Close();
            connectToServer();
        }
    }

    // RECEIVING CODE

    private void Update()
    {
        try
        {
            if (_client.Available > 0)
            {
                byte[] inBytes = StreamUtil.Read(_client.GetStream());
                string inString = Encoding.UTF8.GetString(inBytes);
                Debug.Log("Received:" + inString);

                //how to parse this?? We assumme comma separated string:
                string[] inStringInParts = inString.Split(',');
                if (inStringInParts[0] == "highscores") handleHighScores(inStringInParts);
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


    private void handleHighScores(string[] pInStringInParts)
    {
        _scores.Clear();

        //parse the highscores from the string (assuming the input string is correct)
        int highScoreCount = int.Parse(pInStringInParts[1]);

        for (int i = 0; i < highScoreCount; i++)
        {
            _scores.Add(new Score(pInStringInParts[2 + i*2], int.Parse(pInStringInParts[3 + i*2])));
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

