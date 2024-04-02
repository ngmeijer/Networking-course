using System.Collections.Generic;
using UnityEngine;

/**
 * Demonstrates a simple high score view (no tcp functionality yet, so don't mind the name)
 */
public class TcpHighScoreClient : MonoBehaviour
{
    [SerializeField] private HighScoreView _highScoreView = null;

    private List<Score> _scores = new List<Score>();
    private Score _lastAddedPlayerScore = null;

    void Start()
    {
        _highScoreView.OnScoreAdded += onScoreAdded;
        _highScoreView.OnScoresRequested += onScoresRequested;
    }

    private void onScoreAdded(Score pScore)
    {
        Debug.Log("Added a Score object to the list");

        _scores.Add(pScore);
        _lastAddedPlayerScore = pScore;
    }

    private void onScoresRequested()
    {
        Debug.Log("'Requested' a display of all scores.");

        _scores.Sort((b, a) => a.score.CompareTo(b.score));

        _highScoreView.SetPlayerScoreHeader((_scores.Count > 0 && _scores[0] == _lastAddedPlayerScore) ? "!!! NEW HIGHSCORE !!!" : "YOUR SCORE");
        _highScoreView.SetPlayerScore(_lastAddedPlayerScore);
        _highScoreView.SetHighScores(_scores);
    }

}

