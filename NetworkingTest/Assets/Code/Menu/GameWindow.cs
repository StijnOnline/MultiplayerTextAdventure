using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Assets.Code {
    public class GameWindow : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI outputText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI goldText;

        public void Move(int dir) {

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
    }
}