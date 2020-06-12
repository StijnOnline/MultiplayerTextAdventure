using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Code.Client;

namespace Assets.Code {
    public class GameWindow : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI outputText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI goldText;

        public void Move(int dir) {
            ClientManager.SingleTon.Move(dir);
        }

        public void ExitDungeon() {

        }

        public void CollectTreasure() {

        }

        public void Attack() {

        }

        public void Defend() {

        }

        public void OutputText(string text) {
            outputText.text += text + "\n";
        }

        public void OutputRoomInfoText(RoomInfoMessage roomInfoMessage) {
            string roomInfoText = "The room";

            if(roomInfoMessage.ContainsMonster) {
                roomInfoText += " has a monster";
                if(roomInfoMessage.TreasureInRoom > 0)
                    roomInfoText += " gaurding " + roomInfoMessage.TreasureInRoom + " gold";
            } else if(roomInfoMessage.TreasureInRoom > 0) {
                roomInfoText += " has " + roomInfoMessage.TreasureInRoom + " gold laying around";
            } else {
                roomInfoText += " is empty";
            }


            if(roomInfoMessage.ContainsExit) roomInfoText += ", has the dungeon exit";
            if((roomInfoMessage.directions & Server.Game.Directions.North) != 0) roomInfoText += ", has a North exit";
            if((roomInfoMessage.directions & Server.Game.Directions.East) != 0) roomInfoText += ", has a East exit";
            if((roomInfoMessage.directions & Server.Game.Directions.South) != 0) roomInfoText += ", has a South exit";
            if((roomInfoMessage.directions & Server.Game.Directions.West) != 0) roomInfoText += ", has a West exit";

            if(roomInfoMessage.NumberofOtherPlayers > 0) {
                roomInfoText += ". Also here ";
                if(roomInfoMessage.NumberofOtherPlayers > 1)
                    roomInfoText += "are ";
                else
                    roomInfoText += "is ";
                for(int i = 0; i < roomInfoMessage.NumberofOtherPlayers; i++) {
                    if(i != 0)
                        roomInfoText += " and ";
                    roomInfoText += ClientManager.SingleTon.lobby.players[roomInfoMessage.OtherPlayerIDs[i]].name;
                }
            }
            OutputText(roomInfoText);
        }
    }
}