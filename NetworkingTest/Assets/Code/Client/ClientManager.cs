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

        internal void ExitDungeon() {
            clientBehaviour.QeueMessage(new LeaveDungeonRequestMessage());
        }

        internal void CollectTreasure() {
            clientBehaviour.QeueMessage(new ClaimTreasureRequestMessage());
        }

        internal void Attack() {
            clientBehaviour.QeueMessage(new AttackRequestMessage());
        }

        internal void Defend() {
            clientBehaviour.QeueMessage(new DefendRequestMessage());
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

        public void HandleRequestDenied(Message message) {
            RequestDeniedMessage requestDeniedMessage = (RequestDeniedMessage)message;
            Menu.Singleton.gameWindow.OutputText("Request Denied: Whatever you did is not allowed");
        }

        public void HandleStartGame(Message message) {
            StartGameMessage startGameMessage = (StartGameMessage)message;
            Debug.Log("Game Started");
            Menu.Singleton.SetMenu(Menu.Menus.clientGame);
            Menu.Singleton.gameWindow.UpdateHP(startGameMessage.startHealth);
            Menu.Singleton.gameWindow.UpdateGold(0);
        }

        public void HandlePlayerTurn(Message message) {
            PlayerTurnMessage playerTurnMessage = (PlayerTurnMessage)message;
            
            Menu.Singleton.gameWindow.OutputText("Current turn: " + lobby.players[playerTurnMessage.PlayerID].PlayerTextWithColorTag());
        }

        internal void HandleObtainTreasure(Message message) {
            ObtainTreasureMessage obtainTreasureMessage = (ObtainTreasureMessage)message;
            lobby.You.Value.gold += obtainTreasureMessage.gold;
            Menu.Singleton.gameWindow.OutputText("Obtained " + obtainTreasureMessage.gold + " gold");
            Menu.Singleton.gameWindow.UpdateGold(lobby.You.Value.gold);
        }

        internal void HandleHitMonster(Message message) {
            HitMonserMessage hitMonserMessage = (HitMonserMessage)message;
            Menu.Singleton.gameWindow.OutputText(lobby.players[hitMonserMessage.PlayerID].PlayerTextWithColorTag() + " Hit a monster for " + hitMonserMessage.damage + " damage");
        }

        internal void HandleHitByMonster(Message message) {
            HitByMonserMessage hitByMonserMessage = (HitByMonserMessage)message;
            Menu.Singleton.gameWindow.OutputText(lobby.players[hitByMonserMessage.PlayerID].PlayerTextWithColorTag() + " was hit by a monster, current health is " + hitByMonserMessage.newHP);
            if(hitByMonserMessage.PlayerID == lobby.You.Key)
                Menu.Singleton.gameWindow.UpdateHP(hitByMonserMessage.newHP);
            
        }

        internal void HandlePlayerDefends(Message message) {
            PlayerDefendsMessage playerDefendsMessage = (PlayerDefendsMessage)message;
            Menu.Singleton.gameWindow.OutputText(lobby.players[playerDefendsMessage.PlayerID].PlayerTextWithColorTag() + " defended and regenerated health, current health is " + playerDefendsMessage.newHP);
            if(playerDefendsMessage.PlayerID == lobby.You.Key)
                Menu.Singleton.gameWindow.UpdateHP(playerDefendsMessage.newHP);
        }

        internal void HandlePlayerLeftDungeon(Message message) {
            PlayerLeftDungeonMessage playerLeftDungeonMessage = (PlayerLeftDungeonMessage)message;
            Menu.Singleton.gameWindow.OutputText(lobby.players[playerLeftDungeonMessage.PlayerID].PlayerTextWithColorTag() + " left the dungeon");
        }

        internal void HandlePlayerDies(Message message) {
            PlayerDiesMessage playerDiesMessage = (PlayerDiesMessage)message;
            Menu.Singleton.gameWindow.OutputText(lobby.players[playerDiesMessage.PlayerID].PlayerTextWithColorTag() + " died in the dungeon");
        }

        

        public void HandleRoomInfo(Message message) {
            RoomInfoMessage roomInfoMessage = (RoomInfoMessage)message;
            Menu.Singleton.gameWindow.OutputRoomInfoText(roomInfoMessage);

            //could disable UI
        }

        public void HandlePlayerEnterRoom(Message message) {
            PlayerEnterRoomMessage playerEnterRoomMessage = (PlayerEnterRoomMessage)message;
            Menu.Singleton.gameWindow.OutputText("Player entered room:" + lobby.players[playerEnterRoomMessage.PlayerID].PlayerTextWithColorTag());
        }

        public void HandlePlayerLeaveRoom(Message message) {
            PlayerLeaveRoomMessage playerLeaveRoomMessage = (PlayerLeaveRoomMessage)message;
            Menu.Singleton.gameWindow.OutputText("Player left room:" + lobby.players[playerLeaveRoomMessage.PlayerID].PlayerTextWithColorTag());
        }

        public void Move(int dir) {
            clientBehaviour.QeueMessage(new MoveRequestMessage() { direction = (Server.Game.Directions)dir });
        }

        public void HandleEndGame(Message message) {
            EndGameMessage endGameMessage = (EndGameMessage)message;
            Menu.Singleton.SetMenu(Menu.Menus.score);
            Menu.Singleton.score.SetLastGameScores(lobby, endGameMessage.scoreData);

        }
    }
}
