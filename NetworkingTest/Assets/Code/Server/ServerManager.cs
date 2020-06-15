using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.Server {
    public class ServerManager : MonoBehaviour{
        public static ServerManager Singleton;
        public void Awake() {
            if(Singleton != null && Singleton != this) {
                Destroy(Singleton);
            } else {
                Singleton = this;
            }

            DontDestroyOnLoad(gameObject);
        }

        public Lobby lobby = new Lobby();
        public Game game;
        public ServerBehaviour serverBehaviour { get; private set; }
        internal const int maxPlayers = 4;

        private void Start() {
            serverBehaviour.Start();
        }

        private void Update() {
            serverBehaviour.Update();
        }

        private void OnDestroy() {
            serverBehaviour.Dispose();
        }

        public ServerManager() {
            serverBehaviour = new ServerBehaviour(this);
            game = new Game(this);

        }

        public void StartGame() {
            if(game.started || lobby.players.Count == 0) return;
            game.Start();
            SendStart();
        }

        public void SendStart() {
            foreach(var p in lobby.players) {
                serverBehaviour.QeueMessage(new AdressedMessage(new StartGameMessage() { startHealth = p.Value.health }, p.Key));
                serverBehaviour.QeueMessage(new AdressedMessage(game.GetRoomInfo(p.Key), p.Key));
            }
            SendTurn(game.GetCurrentTurnPlayerID());
        }

        public void SendTurn(int playerID) {
            foreach(var p in lobby.players) {
                serverBehaviour.QeueMessage(new AdressedMessage(new PlayerTurnMessage() { PlayerID = playerID }, p.Key));
            }            
        }

        //General checks before a request is processed
        public bool ValidateMessage(int connectionID) {
            return game.started && connectionID == game.GetCurrentTurnPlayerID();
        }

        public void SendRequestDenied(AdressedMessage adressedMessage) {
            serverBehaviour.QeueMessage(new AdressedMessage(new RequestDeniedMessage() { MessageID = adressedMessage.message.ID }, adressedMessage.connectionID));
        }

        public void HandleNewConnection(int connectionID) {
            //welcome to new player
            var color = new Color(
                UnityEngine.Random.Range(0.2f, 0.5f),
                UnityEngine.Random.Range(0.2f, 0.5f),
                UnityEngine.Random.Range(0.2f, 0.5f)
            );
            var color32 = (Color32)color;
            Debug.Log(color32);
            var welcomeMessage = new WelcomeMessage {
                PlayerID = connectionID,
                Color = ((uint)color32.r << 24) | ((uint)color32.g << 16) | ((uint)color32.b << 8) | color32.a
            };

            serverBehaviour.QeueMessage(new AdressedMessage(welcomeMessage, connectionID));

            //send lobby to new player
            foreach(KeyValuePair<int, Player> player in lobby.players) {
                if(player.Key == connectionID) continue;

                NewPlayerMessage newPlayerMessage = new NewPlayerMessage {
                    PlayerID = player.Key,
                    PlayerColor = ColorExtentions.ToUInt(player.Value.color),
                    PlayerName = player.Value.name
                };
                serverBehaviour.QeueMessage(new AdressedMessage(newPlayerMessage, connectionID));
            }

            lobby.players.Add(connectionID, new Player(color));
        }

        public void HandleDisconnect(int connectionID) {
            PlayerLeftMessage playerLeftMessage = new PlayerLeftMessage {
                PlayerID = connectionID
            };

            lobby.players.Remove(connectionID);

            //send left player to lobby
            foreach(KeyValuePair<int, Player> player in lobby.players) {
                serverBehaviour.QeueMessage(new AdressedMessage(playerLeftMessage, player.Key));
            }
        }

        public void HandleSetName(AdressedMessage adressedMessage) {
            if(game.started) {
                SendRequestDenied(adressedMessage);
                return; 
            }

            SetNameMessage setNameMessage = (SetNameMessage)adressedMessage.message;
            lobby.players[adressedMessage.connectionID].name = setNameMessage.Name;
            int connectionID = adressedMessage.connectionID;
            NewPlayerMessage newPlayerMessage = new NewPlayerMessage {
                PlayerID = connectionID,
                PlayerColor = ColorExtentions.ToUInt(lobby.players[adressedMessage.connectionID].color),
                PlayerName = lobby.players[adressedMessage.connectionID].name
            };

            //send new player to lobby
            foreach(KeyValuePair<int, Player> player in lobby.players) {
                if(player.Key == connectionID) continue;
                serverBehaviour.QeueMessage(new AdressedMessage(newPlayerMessage, player.Key));
            }

        }

        public void HandleMoveRequest(AdressedMessage adressedMessage) {
            if(!ValidateMessage(adressedMessage.connectionID)) {
                SendRequestDenied(adressedMessage);
                return;
            }
            RoomInfoMessage leftRoom = game.GetRoomInfo(adressedMessage.connectionID);


            if(game.MovePlayer(adressedMessage.connectionID, ((MoveRequestMessage)adressedMessage.message).direction)) {

            //player left messages 
                foreach(int playerID in leftRoom.OtherPlayerIDs) {
                    serverBehaviour.QeueMessage(new AdressedMessage(new PlayerLeaveRoomMessage() { PlayerID = adressedMessage.connectionID }, playerID));
                }

                //player entered messages 
                RoomInfoMessage enteredRoom = game.GetRoomInfo(adressedMessage.connectionID);
                foreach(int playerID in enteredRoom.OtherPlayerIDs) {
                    serverBehaviour.QeueMessage(new AdressedMessage(new PlayerEnterRoomMessage() { PlayerID = adressedMessage.connectionID }, playerID));
                }
                //roominfo message
                serverBehaviour.QeueMessage(new AdressedMessage(enteredRoom, adressedMessage.connectionID));
            } else {
                SendRequestDenied(adressedMessage);
            }

            SendTurn(game.GetCurrentTurnPlayerID());
        }

        public void HandleAttackRequest(AdressedMessage adressedMessage) {
            if(!ValidateMessage(adressedMessage.connectionID)) {
                SendRequestDenied(adressedMessage); return;
            }

            if(game.Attack(adressedMessage.connectionID)) {

                
            } else {
                SendRequestDenied(adressedMessage);
            }

        }

        public void SendHitMonsterMessage(int connectionID, ushort damage) {
            foreach(KeyValuePair<int, Player> player in lobby.players) {
                serverBehaviour.QeueMessage(new AdressedMessage(new HitMonserMessage() { PlayerID = connectionID, damage = damage }, connectionID));
            }
        }

        public void SendHitByMonsterMessage(int connectionID, ushort newHP) {
            foreach(KeyValuePair<int, Player> player in lobby.players) {
                serverBehaviour.QeueMessage(new AdressedMessage(new HitByMonserMessage() { PlayerID = connectionID , newHP = newHP}, connectionID));
            }
        }

        public void SendPlayerDefendsMessage(int connectionID, ushort newHP) {
            foreach(KeyValuePair<int, Player> player in lobby.players) {
                serverBehaviour.QeueMessage(new AdressedMessage(new PlayerDefendsMessage() { PlayerID = connectionID, newHP = newHP }, connectionID));
            }
        }

        internal void SendObtainGold(ushort v) {
            throw new NotImplementedException();
        }

        public void SendPlayerDiedMessage(int connectionID) {
            foreach(KeyValuePair<int, Player> player in lobby.players) {
                serverBehaviour.QeueMessage(new AdressedMessage(new PlayerDiesMessage() { PlayerID = connectionID }, connectionID));
            }
        }

        public void HandleDefendRequest(AdressedMessage adressedMessage) {
            if(!ValidateMessage(adressedMessage.connectionID)) {
                SendRequestDenied(adressedMessage); return;
            }

            if( ! game.Defend(adressedMessage.connectionID))
                SendRequestDenied(adressedMessage);
            
        }

        public void HandleClaimTreasureRequest(AdressedMessage adressedMessage) {
            if(!ValidateMessage(adressedMessage.connectionID)) {
                SendRequestDenied(adressedMessage); return;
            }

            if(!game.ClaimTreasure(adressedMessage.connectionID))
                SendRequestDenied(adressedMessage);
        }

        internal void EndGame() {
            throw new NotImplementedException();
        }

        public void HandleLeaveDungeonRequest(AdressedMessage adressedMessage) {
            if(!ValidateMessage(adressedMessage.connectionID)) {
                SendRequestDenied(adressedMessage); return;
            }

            if(!game.LeaveDungeon(adressedMessage.connectionID))
                SendRequestDenied(adressedMessage);
        }

        public void SendObtainGold(int connectionID, ushort gold) {
            serverBehaviour.QeueMessage(new AdressedMessage(new ObtainTreasureMessage { gold = gold }, connectionID));
        }

        public void SendPlayerLeftDungeon(int connectionID) {
            serverBehaviour.QeueMessage(new AdressedMessage(new PlayerLeftDungeonMessage { PlayerID = connectionID }, connectionID));
        }


    }
}