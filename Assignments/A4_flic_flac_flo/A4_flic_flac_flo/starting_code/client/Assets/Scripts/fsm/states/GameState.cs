using shared;
using shared.src.protocol.Lobby;
using System;
using TMPro;
using UnityEngine;

/**
 * This is where we 'play' a game.
 */
public class GameState : ApplicationStateWithView<GameView>
{
    //just for fun we keep track of how many times a player clicked the board
    //note that in the current application you have no idea whether you are player 1 or 2
    //normally it would be better to maintain this sort of info on the server if it is actually important information
    private int _player1MoveCount = 0;
    private int _player2MoveCount = 0;

    private string _player1Name;
    private string _player2Name;

    public override void EnterState()
    {
        base.EnterState();
        
        view.gameBoard.OnCellClicked += _onCellClicked;
    }

    private void _onCellClicked(int pCellIndex)
    {
        MakeMoveRequest makeMoveRequest = new MakeMoveRequest();
        makeMoveRequest.move = pCellIndex;

        fsm.channel.SendMessage(makeMoveRequest);
    }

    public override void ExitState()
    {
        base.ExitState();
        view.gameBoard.OnCellClicked -= _onCellClicked;
    }

    private void Update()
    {
        receiveAndProcessNetworkMessages();
    }

    protected override void handleNetworkMessage(ASerializable pMessage)
    {
        switch (pMessage)
        {
            case MakeMoveResult:
                handleMakeMoveResult(pMessage as MakeMoveResult);
                break;
            case PlayerNameUpdate:
                handleNameUpdate(pMessage as PlayerNameUpdate);
                break;
            case RoomJoinedEvent:
                handleRoomJoinedEvent(pMessage as RoomJoinedEvent);
                break;
        }
    }

    private void handleNameUpdate(PlayerNameUpdate pPlayerNameUpdate)
    {
        Debug.Log($"Player1 name: {pPlayerNameUpdate.Player1Name}");
        _player1Name = pPlayerNameUpdate.Player1Name;
        _player2Name = pPlayerNameUpdate.Player2Name;

        updateLabelText(view.playerLabel1, $"P1 {_player1Name} (Movecount: {_player1MoveCount})");
        updateLabelText(view.playerLabel2, $"P2 {_player2Name} (Movecount: {_player2MoveCount})");
    }

    private void updateLabelText(TMP_Text pTextObject, string pContent)
    {
        pTextObject.text = pContent;
    }

    private void handleMakeMoveResult(MakeMoveResult pMakeMoveResult)
    {
        view.gameBoard.SetBoardData(pMakeMoveResult.boardData);

        //some label display
        if (pMakeMoveResult.whoMadeTheMove == 1)
        {
            _player1MoveCount++;
            updateLabelText(view.playerLabel1, $"P1 {_player1Name} (Movecount: {_player1MoveCount})");
        }
        if (pMakeMoveResult.whoMadeTheMove == 2)
        {
            _player2MoveCount++;
            updateLabelText(view.playerLabel2, $"P2 {_player2Name} (Movecount: {_player2MoveCount})");
        }
    }

        private void handleRoomJoinedEvent (RoomJoinedEvent pMessage)
    {
        if (pMessage.room == RoomJoinedEvent.Room.LOBBY_ROOM)
        {
            fsm.ChangeState<LobbyState>();
        } 
    }
}
