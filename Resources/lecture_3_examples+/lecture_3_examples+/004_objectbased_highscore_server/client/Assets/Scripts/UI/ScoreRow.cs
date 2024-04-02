using UnityEngine;
using UnityEngine.UI;

public class ScoreRow : MonoBehaviour
{
    [SerializeField] private Text _nameField = null;
    [SerializeField] private Text _scoreField = null;

    public void ShowScore (string pLabel, string pScore)
    {
        _nameField.text = pLabel;
        _scoreField.text = pScore;
    }
}
