using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.Client {
    public class ClientManager {
        private Lobby lobby = new Lobby();
        private ClientBehaviour clientBehaviour;
        public ClientManager(ClientBehaviour clientBehaviour) {
            this.clientBehaviour = clientBehaviour;
        }


        public void HandleNewPlayer(Message message) {
            NewPlayerMessage newPlayerMessage = (NewPlayerMessage)message;
            Player newPlayer = new Player(newPlayerMessage.PlayerColor);
            newPlayer.name = newPlayerMessage.PlayerName;
            lobby.players.Add(newPlayerMessage.PlayerID, newPlayer);
            Menu.Instance.LobbyWindow.UpdatePlayers(lobby);
        }

        public void HandlePlayerLeft(Message message) {
            PlayerLeftMessage playerLeftMessage = (PlayerLeftMessage)message;
            lobby.players.Remove(playerLeftMessage.PlayerID);
        }

        public void HandleWelcome(Message message) {
            WelcomeMessage welcomeMessage = (WelcomeMessage) message;
            Debug.Log(welcomeMessage.Color);
            Player newPlayer = new Player(welcomeMessage.Color);

            newPlayer.name = Login.Username;
            lobby.SetPlayer(welcomeMessage.PlayerID, newPlayer);
            Menu.Instance.LobbyWindow.UpdatePlayers(lobby);
            //immediately set player name
            var setNameMessage = new SetNameMessage {
                Name = Login.Username
            };
            clientBehaviour.QeueMessage(setNameMessage);
        }
    }
}
