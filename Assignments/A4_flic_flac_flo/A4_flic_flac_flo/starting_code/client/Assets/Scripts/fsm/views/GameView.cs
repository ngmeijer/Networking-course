using shared;
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

    [SerializeField] private TMP_Text _winnerText;
    public TMP_Text winnerText => _winnerText;

    [SerializeField] private TMP_Text _surrenderedText;
    public TMP_Text surrenderedText => _surrenderedText;

    [SerializeField] private TMP_Text _player1MoveCountText;
    [SerializeField] private TMP_Text _player2MoveCountText;

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

        _gameEndScreen.SetActive(false);
    }

    public void EnableEndScreen()
    {
        _gameEndScreen.SetActive(true);
    }

    public void UpdateEndScreenData(TicTacToeBoardData pData)
    {
        int hasSurrendered = pData.WhoHasSurrendered();
        int whoWon = pData.WhoHasWon();

        _player1MoveCountText.SetText($"Player 1 move count: {pData.Player1.MoveCount}");
        _player2MoveCountText.SetText($"Player 2 move count: {pData.Player2.MoveCount}");

        if (hasSurrendered != 0)
        {
            if (hasSurrendered == 1)
            {
                surrenderedText.SetText($"Player 1 '{pData.Player1.PlayerName} has surrendered.'");
                winnerText.SetText($"Player 2 '{pData.Player2.PlayerName}' has won!");
            }
            else if (hasSurrendered == 2)
            {
                surrenderedText.SetText($"Player 2 '{pData.Player2.PlayerName} has surrendered.'");
                winnerText.SetText($"Player 1 '{pData.Player1.PlayerName}' has won!");
            }

            return;
        }

        surrenderedText.SetText("");
        if (whoWon == 1)
        {
            winnerText.SetText($"Player 1 '{pData.Player1.PlayerName}' has won!");
        }
        else if (whoWon == 2)
        {
            winnerText.SetText($"Player 2 '{pData.Player2.PlayerName}' has won!");
        }
    }

    public void UpdateLabelText(TMP_Text pTextObject, string pPlayerInfo, int pMoveCount)
    {
        pTextObject.text = $"{pPlayerInfo} (Movecount: {pMoveCount})";
    }

    public void ResetUI()
    {
        gameBoard.SetBoardData(new TicTacToeBoardData());
        _gameEndScreen.SetActive(false);
        UpdateLabelText(playerLabel2, $"P1 '{""}'", 0);
        UpdateLabelText(playerLabel2, $"P2 '{""}'", 0);
    }
}

