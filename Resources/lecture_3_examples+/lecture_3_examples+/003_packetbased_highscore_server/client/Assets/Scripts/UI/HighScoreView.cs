using shared;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Wrapper around the HighScore UI, 
 */
public class HighScoreView : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////
    ///
    ///                     References to all UI elements

    [SerializeField] private Text _textPlayerScoreHeader = null;
    [SerializeField] private ScoreRow _playerScoreRow = null;
    [SerializeField] private List<ScoreRow> _highScoreRows = null;
    
    [SerializeField] private InputField _nameInput = null;
    [SerializeField] private InputField _scoreInput = null;
    [SerializeField] private Button _addButton = null;
    [SerializeField] private Button _getButton = null;

    //Some fields for custom tab navigation, which is broken otherwise
    private Selectable[] _fields;
    private int _fieldIndexer = 0;
    
    //the event you'll want to listen to for scores to be added or requested
    public event Action<Score> OnScoreAdded = delegate { };
    public event Action OnScoresRequested = delegate { };

    private void Awake()
    {
        _addButton.onClick.AddListener(dispatchScore);
        _getButton.onClick.AddListener(() => OnScoresRequested());

        _fields = new Selectable[] { _nameInput, _scoreInput, _addButton};
    }

    public void SetPlayerScoreHeader(string pPlayerScoreHeader)
    {
        _textPlayerScoreHeader.text = pPlayerScoreHeader;
    }

    public void SetPlayerScore (Score pPlayerScore)
    {
        if (pPlayerScore != null)
        {
            _playerScoreRow.ShowScore(pPlayerScore.name, "" + pPlayerScore.score);
        } else
        {
            _playerScoreRow.ShowScore("---", "---");
        }
    }

    public void SetHighScores (List<Score> pHighScores)
    {
        int scoreRowCount = (_highScoreRows != null) ? _highScoreRows.Count : 0;
        int scoreCount = (pHighScores != null) ? pHighScores.Count : 0;

        //update the highscore or clear them, based on the given input
        for (int i = 0; i < scoreRowCount;i++)
        {
            if (i < scoreCount)
            {
                _highScoreRows[i].ShowScore("" + (i + 1) + ". " + pHighScores[i].name, "" + pHighScores[i].score);
            } else
            {
                _highScoreRows[i].ShowScore("" + (i + 1) + ". ---", "---");
            }
        }
    }

    private void dispatchScore ()
    {
        //minor sanity checks
        if (_nameInput.text.Length == 0) return;
        if (_scoreInput.text.Length == 0) return;
        OnScoreAdded(new Score(_nameInput.text, int.Parse(_scoreInput.text)));
    }

    private void Update()
    {
        handleTabNavigation();
    }

    private void handleTabNavigation()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                _fieldIndexer--;
            }
            else
            {
                _fieldIndexer++;
            }
            _fieldIndexer = Mathf.Clamp(_fieldIndexer, 0, _fields.Length - 1);

            _fields[_fieldIndexer].Select();
        }
    }
}
