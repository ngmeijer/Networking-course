﻿using shared;
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

        _player1MoveCount = 0;
        _player2MoveCount = 0;
        view.ResetUI();
        view.gameBoard.OnCellClicked += _onCellClicked;

        //Notify the GameRoom a player has surrendered.
        view.OnSurrenderedClicked += _onSurrenderClicked;
        view.OnClickedReturnToLobby += _onReturnToLobbyClicked;
    }

    private void _onCellClicked(int pCellIndex)
    {
        MakeMoveRequest makeMoveRequest = new MakeMoveRequest();
        makeMoveRequest.move = pCellIndex;

        fsm.channel.SendMessage(makeMoveRequest);
    }

    private void _onSurrenderClicked()
    {
        SurrenderRequest surrenderRequest = new SurrenderRequest();
        fsm.channel.SendMessage(surrenderRequest);
    }

    private void _onReturnToLobbyClicked()
    {
        LeaveGameRoomRequest leaveGameRoomRequest = new LeaveGameRoomRequest();
        fsm.channel.SendMessage(leaveGameRoomRequest);
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
            case PlayerInfo:
                handleNameUpdate(pMessage as PlayerInfo);
                break;
            case RoomJoinedEvent:
                handleRoomJoinedEvent(pMessage as RoomJoinedEvent);
                break;
            case TicTacToeBoardData:
                handleGameEnd();
                handleDataUpdate(pMessage as TicTacToeBoardData);
                break;
        }
    }

    private void handleGameEnd()
    {
        view.EnableEndScreen();
    }

    private void handleDataUpdate(TicTacToeBoardData pData)
    {
        view.UpdateEndScreenData(pData);
    }

    private void handleNameUpdate(PlayerInfo pPlayerInfo)
    {
        if(pPlayerInfo.PlayerId == 1)
            _player1Name = pPlayerInfo.PlayerName;
        if(pPlayerInfo.PlayerId == 2)
            _player2Name = pPlayerInfo.PlayerName;

        view.UpdateLabelText(view.playerLabel1, $"P1 '{_player1Name}'", _player1MoveCount);
        view.UpdateLabelText(view.playerLabel2, $"P2 '{_player2Name}'", _player2MoveCount);
    }

    private void handleMakeMoveResult(MakeMoveResult pMakeMoveResult)
    {
        view.gameBoard.SetBoardData(pMakeMoveResult.boardData);

        //some label display
        if (pMakeMoveResult.whoMadeTheMove == 1)
        {
            _player1MoveCount++;
            view.UpdateLabelText(view.playerLabel1, $"P1 {_player1Name}", _player1MoveCount);
        }
        if (pMakeMoveResult.whoMadeTheMove == 2)
        {
            _player2MoveCount++;
            view.UpdateLabelText(view.playerLabel2, $"P2 {_player2Name}", _player2MoveCount);
        }
    }

    private void handleRoomJoinedEvent(RoomJoinedEvent pMessage)
    {
        if (pMessage.room == RoomJoinedEvent.Room.LOBBY_ROOM)
        {
            fsm.ChangeState<LobbyState>();
        }
    }
}
