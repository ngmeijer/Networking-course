using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * Wraps all elements and functionality required for the GameView.
 */
public class GameView : View
{
    [SerializeField] private GameBoard _gameboard = null;
    public GameBoard gameBoard => _gameboard;
    [SerializeField] private TMP_Text _player1Label = null;
    public TMP_Text playerLabel1 => _player1Label;
    [SerializeField] private TMP_Text _player2Label = null;
    public TMP_Text playerLabel2 => _player2Label;

    [SerializeField] private Button _surrenderButton;
    public Button surrenderButton => _surrenderButton;
    public event Action OnSurrenderedClicked = delegate { };

    [SerializeField] private GameObject _gameEndScreen;
    public GameObject gameEndScreen => _gameEndScreen;

    [SerializeField] private Button _returnToLobbyButton;
    public Button returnToLobbyButton => _returnToLobbyButton;
    public event Action OnClickedReturnToLobby = delegate { };

    private void Start()
    {
        //Notify the GameState about surrendering
        _surrenderButton.onClick.AddListener(() =>
        {
            OnSurrenderedClicked();
        });

        _returnToLobbyButton.onClick.AddListener(() =>
        {
            OnClickedReturnToLobby();
        });
    }

    public void EnableEndScreen()
    {
        _gameEndScreen.SetActive(true);
    }
}

