using shared;
using shared.src.protocol.Lobby;
using System;

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

		private PlayerInfo _player1 = new PlayerInfo();
		private PlayerInfo _player2 = new PlayerInfo();

		public GameRoom(TCPGameServer pOwner) : base(pOwner)
		{
		}

		public void StartGame (TcpMessageChannel pPlayer1, TcpMessageChannel pPlayer2)
		{
			if (IsGameInPlay) throw new Exception("Programmer error duuuude.");

			IsGameInPlay = true;

			_player1.PlayerName = pPlayer1.Name;
			_player2.PlayerName = pPlayer2.Name;
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
				_player1.PlayerName,
				_player2.PlayerName
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
		}

		private void handleMakeMoveRequest(MakeMoveRequest pMessage, TcpMessageChannel pSender)
		{
			//we have two players, so index of sender is 0 or 1, which means playerID becomes 1 or 2
			int playerID = indexOfMember(pSender) + 1;
			//make the requested move (0-8) on the board for the player
			_board.MakeMove(pMessage.move, playerID);

			//and send the result of the boardstate back to all clients
			MakeMoveResult makeMoveResult = new MakeMoveResult();
			makeMoveResult.whoMadeTheMove = playerID;
			makeMoveResult.boardData = _board.GetBoardData();
			sendToAll(makeMoveResult);

		    checkGameEnd();
		}

		private void checkGameEnd()
		{
			TicTacToeBoardData data = _board.GetBoardData();
			int whoWon = data.WhoHasWon();
			if (whoWon == 0)
				return;

			//Remove members from game room
			TcpMessageChannel player1 = GetMember(0);
			TcpMessageChannel player2 = GetMember(1);

			removeMember(player1);
			removeMember(player2);

			_server.GetLobbyRoom().DeleteGameRoom(this);
			//Add members to lobby room
			LobbyRoom lobbyRoom = _server.GetLobbyRoom();
            lobbyRoom.AddMember(player1);
            lobbyRoom.AddMember(player2);

			data.Player1 = _player1;
			data.Player2 = _player2;
			lobbyRoom.HandleFinishedGame(data);

            RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
            roomJoinedEvent.room = RoomJoinedEvent.Room.LOBBY_ROOM;
            player1.SendMessage(roomJoinedEvent);
			player2.SendMessage(roomJoinedEvent);
		}
    }
}
