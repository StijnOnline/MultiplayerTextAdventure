using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

namespace Assets.Code {

    public class LobbyWindow : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI[] playerTextBoxes;

        public void UpdatePlayers(Lobby lobby) {
            int playerCount = 0;
            foreach(KeyValuePair<int,Player> playerKeyPair in lobby.players) {
                playerTextBoxes[playerCount].SetText(playerKeyPair.Value.name);
                playerTextBoxes[playerCount].fontStyle = (lobby.you == playerKeyPair.Key ? FontStyles.Underline:FontStyles.Normal);
                playerCount++;
            }
        }
    }
}