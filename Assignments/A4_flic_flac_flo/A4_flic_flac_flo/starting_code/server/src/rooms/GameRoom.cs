using shared;
using shared.src.protocol.Lobby;
using System;
using System.Collections.Generic;

namespace server
{
	/**
	 * This room runs a single Game (at a time). 
	 * 
	 * The 'Game' is very simple at the moment:
	 *	- all client moves are broadcasted to all clients
	 *	
	 * The game has no end yet (that is up to you), in other words:
	 * all players that are added to this room, stay in here indefinitely.
	 */
	class GameRoom : Room
	{
		public bool IsGameInPlay { get; private set; }

		//wraps the board to play on...
		private TicTacToeBoard _board = new TicTacToeBoard();

		private PlayerInfo _player1Info = new PlayerInfo();
		private PlayerInfo _player2Info = new PlayerInfo();

		private TcpMessageChannel _player1Channel;
		private TcpMessageChannel _player2Channel;

		public GameRoom(TCPGameServer pOwner) : base(pOwner)
		{
		}

		public void StartGame (TcpMessageChannel pPlayer1, TcpMessageChannel pPlayer2)
		{
			if (IsGameInPlay) throw new Exception("Programmer error duuuude.");

			IsGameInPlay = true;

			_player1Channel = pPlayer1;
			_player2Channel = pPlayer2;

			_player1Info.PlayerName = pPlayer1.Name;
			_player2Info.PlayerName = pPlayer2.Name;
            addMember(pPlayer1);
            addMember(pPlayer2);

			PlayerNameUpdate nameUpdate = new PlayerNameUpdate()
			{
				Player1Name = pPlayer1.Name,
				Player2Name = pPlayer2.Name
			};
            pPlayer1.SendMessage(nameUpdate);
            pPlayer2.SendMessage(nameUpdate);
        }

		public string[] GetPlayerNames()
		{
			string[] names =
			{
				_player1Info.PlayerName,
				_player2Info.PlayerName
			};

			return names;
		}

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			//notify client he has joined a game room 
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.GAME_ROOM;
			pMember.SendMessage(roomJoinedEvent);
		}

		public override void Update()
		{
			//demo of how we can tell people have left the game...
			int oldMemberCount = memberCount;
			base.Update();
			int newMemberCount = memberCount;

			if (oldMemberCount != newMemberCount)
			{
				Log.LogInfo("People left the game...", this);
			}
		}

		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is MakeMoveRequest)
			{
				handleMakeMoveRequest(pMessage as MakeMoveRequest, pSender);
			}

			if(pMessage is SurrenderRequest)
			{
				handleSurrenderRequest(pSender);
			}

			if (pMessage is LeaveGameRoomRequest)
			{
				handleLeaveGameRoomRequest(pSender);
			}
		}

        private void handleLeaveGameRoomRequest(TcpMessageChannel pSender)
        {
            TicTacToeBoardData data = _board.GetBoardData();
			data.Player1 = _player1Info;
			data.Player2 = _player2Info;
            handlePlayerLeaving(pSender, data);
        }

        private void handleSurrenderRequest(TcpMessageChannel pSender)
        {
            TicTacToeBoardData data = _board.GetBoardData();
            data.SurrenderedIndex = indexOfMember(pSender) + 1;
			data.Player1 = _player1Info;
			data.Player2 = _player2Info;

			_player1Channel.SendMessage(data);
			_player2Channel.SendMessage(data);
        }

        private void handleMakeMoveRequest(MakeMoveRequest pMessage, TcpMessageChannel pSender)
		{
			//we have two players, so index of sender is 0 or 1, which means playerID becomes 1 or 2
			int playerID = indexOfMember(pSender) + 1;
			if (playerID == 1)
				_player1Info.MoveCount++;
			else if(playerID == 2)
				_player2Info.MoveCount++;
			//make the requested move (0-8) on the board for the player
			_board.MakeMove(pMessage.move, playerID);

			//and send the result of the boardstate back to all clients
			MakeMoveResult makeMoveResult = new MakeMoveResult();
			makeMoveResult.whoMadeTheMove = playerID;
			makeMoveResult.boardData = _board.GetBoardData();
            makeMoveResult.boardData.Player1 = _player1Info;
            makeMoveResult.boardData.Player2 = _player2Info;
            sendToAll(makeMoveResult);

		    checkGameEndConditions();
		}

		private void checkGameEndConditions()
		{
			TicTacToeBoardData data = _board.GetBoardData();

            int whoWon = data.WhoHasWon();

			if (whoWon == 0)
				return;

			data.Player1 = _player1Info;
			data.Player2 = _player2Info;
            _player1Channel.SendMessage(data);
            _player2Channel.SendMessage(data);
        }

		private void handlePlayerLeaving(TcpMessageChannel pSender, TicTacToeBoardData pData)
		{
			//Only delete room if the last/2nd player has left.
			if (this.memberCount == 1)
			{
                removeMember(pSender);
				_server.GetLobbyRoom().DeleteGameRoom(this);
			}
            returnPlayerToLobby(pSender, pData);
        }

		private void returnPlayerToLobby(TcpMessageChannel pSender, TicTacToeBoardData pData)
		{
            //Add members to lobby room
            LobbyRoom lobbyRoom = _server.GetLobbyRoom();
            lobbyRoom.AddMember(pSender);

			//To log the global "player won" message in the lobby.
            pData.Player1 = _player1Info;
            pData.Player2 = _player2Info;
            lobbyRoom.HandleFinishedGame(pData);

			//Notify clients they have to go back to the lobby.
            RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
            roomJoinedEvent.room = RoomJoinedEvent.Room.LOBBY_ROOM;
            pSender.SendMessage(roomJoinedEvent);
        }
    }
}
