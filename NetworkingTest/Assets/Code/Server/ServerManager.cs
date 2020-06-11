using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.Server {
    public class ServerManager {
        public Lobby lobby = new Lobby();
        public Game game;
        public ServerBehaviour serverBehaviour;
        internal const int maxPlayers = 4;

        public ServerManager(ServerBehaviour serverBehaviour) {
            this.serverBehaviour = serverBehaviour;
            game = new Game(this);
        }

        public void StartGame() {
            if(game.started) return;
            game.Start();
            foreach(var p in lobby.players) {
                serverBehaviour.QeueMessage(new AdressedMessage(new StartGameMessage(p.Value.health), p.Key));
            }
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
            if(game.started) return;

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
    }
}