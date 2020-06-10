using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

namespace Assets.Code.Server {
    public class ServerManager {
        public Lobby lobby = new Lobby();
        public Game game = new Game();
        public ServerBehaviour serverBehaviour;
        internal const int maxPlayers = 4;

        public ServerManager(ServerBehaviour serverBehaviour) {
            this.serverBehaviour = serverBehaviour;
        }

        public void HandleSetName(AdressedMessage adressedMessage) {
            if(game.started) return;

            SetNameMessage setNameMessage = (SetNameMessage)adressedMessage.message;
            lobby.players[adressedMessage.connectionID].name = setNameMessage.Name;
            Debug.Log("Got a name: " + setNameMessage.Name);
            int connectionID = adressedMessage.connectionID;
            NewPlayerMessage newPlayerMessage = new NewPlayerMessage {
                PlayerID = connectionID,
                PlayerColor = lobby.players[adressedMessage.connectionID].color.ToUInt(),
                PlayerName = lobby.players[adressedMessage.connectionID].name
            };

            //send new player to lobby
            foreach(KeyValuePair<int, Player> player in lobby.players) {
                if(player.Key == connectionID) continue;
                serverBehaviour.QeueMessage(new AdressedMessage(newPlayerMessage, player.Key));
            }

        }

        public void HandleNewConnection(int connectionID) {

            Debug.Log("Sending Welcome");

            //welcome to new player
            var color = Color.magenta; //TODO assign random color
            var color32 = (Color32)color;
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
                    PlayerColor = player.Value.color.ToUInt(),
                    PlayerName = player.Value.name
                };
                serverBehaviour.QeueMessage(new AdressedMessage(newPlayerMessage, connectionID));
            }

            lobby.AddPlayer(connectionID, new Player(color));
        }
    }
}