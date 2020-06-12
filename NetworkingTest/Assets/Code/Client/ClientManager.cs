using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.Client {
    public class ClientManager : MonoBehaviour {
        public static ClientManager SingleTon;
        public void Awake() {
            if(SingleTon != null && SingleTon != this) {
                Destroy(SingleTon);
            } else {
                SingleTon = this;
            }

            DontDestroyOnLoad(gameObject);
        }


        public Lobby lobby { get; private set; } = new Lobby();
        public ClientBehaviour clientBehaviour { get; private set; }

        public ClientManager() {
            clientBehaviour = new ClientBehaviour(this);
        }

        private void Update() {
            clientBehaviour.Update();
        }

        private void OnDestroy() {
            clientBehaviour.Dispose();
        }


        public void HandleNewPlayer(Message message) {
            NewPlayerMessage newPlayerMessage = (NewPlayerMessage)message;
            Player newPlayer = new Player(newPlayerMessage.PlayerColor);
            newPlayer.name = newPlayerMessage.PlayerName;
            lobby.players.Add(newPlayerMessage.PlayerID, newPlayer);
            Menu.Singleton.UpdateLobbyWindows(lobby);
        }

        public void HandlePlayerLeft(Message message) {
            PlayerLeftMessage playerLeftMessage = (PlayerLeftMessage)message;
            lobby.players.Remove(playerLeftMessage.PlayerID);
        }

        public void HandleWelcome(Message message) {
            WelcomeMessage welcomeMessage = (WelcomeMessage)message;
            Player newPlayer = new Player(welcomeMessage.Color);

            newPlayer.name = Login.Username;
            lobby.SetPlayer(welcomeMessage.PlayerID, newPlayer);
            Menu.Singleton.UpdateLobbyWindows(lobby);
            //immediately set player name
            var setNameMessage = new SetNameMessage {
                Name = Login.Username
            };
            clientBehaviour.QeueMessage(setNameMessage);
        }

        public void HandleStartGame(Message message) {
            StartGameMessage startGameMessage = (StartGameMessage)message;
            //set HP in UI
            Debug.Log("Game Started");
            Menu.Singleton.SetMenu(Menu.Menus.clientGame);
        }

        public void HandlePlayerTurn(Message message) {
            PlayerTurnMessage playerTurnMessage = (PlayerTurnMessage)message;
            Menu.Singleton.gameWindow.OutputText("Turn: " + lobby.players[playerTurnMessage.PlayerID].name);
        }

        public void HandleRoomInfo(Message message) {
            RoomInfoMessage roomInfoMessage = (RoomInfoMessage)message;
            Menu.Singleton.gameWindow.OutputRoomInfoText(roomInfoMessage);

            //could disable UI
        }

        public void HandlePlayerEnterRoom(Message message) {
            PlayerEnterRoomMessage playerEnterRoomMessage = (PlayerEnterRoomMessage)message;
            Menu.Singleton.gameWindow.OutputText(lobby.players[playerEnterRoomMessage.PlayerID].name);
        }

        public void HandlePlayerLeaveRoom(Message message) {
            PlayerLeaveRoomMessage playerLeaveRoomMessage = (PlayerLeaveRoomMessage)message;
            Menu.Singleton.gameWindow.OutputText(lobby.players[playerLeaveRoomMessage.PlayerID].name);
        }

        public void Move(int dir) {
            clientBehaviour.QeueMessage(new MoveRequestMessage() { direction = (Server.Game.Directions)dir });
        }
    }
}
