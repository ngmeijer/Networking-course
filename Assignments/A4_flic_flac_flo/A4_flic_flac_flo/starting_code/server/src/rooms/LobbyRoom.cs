﻿using shared;
using System;
using System.Collections.Generic;

namespace server
{
    /**
	 * The LobbyRoom is a little bit more extensive than the LoginRoom.
	 * In this room clients change their 'ready status'.
	 * If enough people are ready, they are automatically moved to the GameRoom to play a Game (assuming a game is not already in play).
	 */
    class LobbyRoom : SimpleRoom
    {
        //this list keeps tracks of which players are ready to play a game, this is a subset of the people in this room
        private List<TcpMessageChannel> _readyMembers = new List<TcpMessageChannel>();


        public LobbyRoom(TCPGameServer pOwner) : base(pOwner)
        {
        }

        protected override void addMember(TcpMessageChannel pMember)
        {
            base.addMember(pMember);

            //tell the member it has joined the lobby
            RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
            roomJoinedEvent.room = RoomJoinedEvent.Room.LOBBY_ROOM;
            pMember.SendMessage(roomJoinedEvent);

            //print some info in the lobby (can be made more applicable to the current member that joined)
            ChatMessage simpleMessage = new ChatMessage();
            simpleMessage.message = $"Client '{pMember.Name}' has joined the lobby!";

            foreach (TcpMessageChannel member in _members)
            {
                member.SendMessage(simpleMessage);
            }

            //send information to all clients that the lobby count has changed
            sendLobbyUpdateCount();
        }

        /**
		 * Override removeMember so that our ready count and lobby count is updated (and sent to all clients)
		 * anytime we remove a member.
		 */
        protected override void removeMember(TcpMessageChannel pMember)
        {
            base.removeMember(pMember);
            _readyMembers.Remove(pMember);

            sendLobbyUpdateCount();
        }

        protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
        {
            if (pMessage is ChangeReadyStatusRequest)
                handleReadyNotification(pMessage as ChangeReadyStatusRequest, pSender);
            if (pMessage is ChatMessage)
                handleChatMessage(pMessage as ChatMessage, pSender);
        }

        private void handleReadyNotification(ChangeReadyStatusRequest pReadyNotification, TcpMessageChannel pSender)
        {
            //if the given client was not marked as ready yet, mark the client as ready
            if (pReadyNotification.ready)
            {
                if (!_readyMembers.Contains(pSender)) _readyMembers.Add(pSender);
            }
            else //if the client is no longer ready, unmark it as ready
            {
                _readyMembers.Remove(pSender);
            }

            //do we have enough people for a game and is there no game running yet?
            if (_readyMembers.Count >= 2)
            {
                TcpMessageChannel player1 = _readyMembers[0];
                TcpMessageChannel player2 = _readyMembers[1];
                removeMember(player1);
                removeMember(player2);

                _server.InitializeGameRoom().StartGame(player1, player2);
            }

            //(un)ready-ing / starting a game changes the lobby/ready count so send out an update
            //to all clients still in the lobby
            sendLobbyUpdateCount();
        }

        private void sendLobbyUpdateCount()
        {
            LobbyInfoUpdate lobbyInfoMessage = new LobbyInfoUpdate();
            lobbyInfoMessage.memberCount = memberCount;
            lobbyInfoMessage.readyCount = _readyMembers.Count;
            sendToAll(lobbyInfoMessage);
        }

        public void NotifyLobbyAboutGameWin(TicTacToeBoardData data)
        {
            ChatMessage gameSummary = new ChatMessage();
            int hasSurrendered = data.WhoHasSurrendered();
            if (hasSurrendered != 0)
            {
                if (hasSurrendered == 1)
                    gameSummary.message = $"Player 1 '{data.Player1.PlayerName}' has surrendered and lost a game to Player 2 '{data.Player2.PlayerName}'!'";
                else if(hasSurrendered == 2)
                    gameSummary.message = $"Player 2 '{data.Player2.PlayerName}' has surrendered and lost a game to Player 1 '{data.Player1.PlayerName}'!'";
                sendToAll(gameSummary);
                return;
            }

            if (data.WhoHasWon() == 1)
                gameSummary.message = $"Player 1 '{data.Player1.PlayerName}' has won a game from Player 2 '{data.Player2.PlayerName}'!";
            else if (data.WhoHasWon() == 2)
                gameSummary.message = $"Player 2 '{data.Player2.PlayerName}' has won a game from Player 1 '{data.Player1.PlayerName}'!";

            sendToAll(gameSummary);
        }

        public void DeleteGameRoom(GameRoom gameRoom)
        {
            _server.AddGameRoomToDeleteList(gameRoom);
        }
    }
}
