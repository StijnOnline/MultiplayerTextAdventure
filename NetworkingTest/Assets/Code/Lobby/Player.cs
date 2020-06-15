using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    public class Player {
        //Game
        public ushort health;
        public ushort gold = 0;
        public Vector2Int position;
        public bool leftDungeon = false;
        public static ushort DEFAULT_DAMAGE = 3;

        public string name { get; set; } = null;
        public Color color {get; private set; }

        public Player(uint color) {
            Color32 color32;
            color32 = ColorExtentions.FromUInt(color);
            this.color = color32;
        }

        public Player(Color color) {
            this.color = color;
        }

        public string PlayerTextWithColorTag() {
            return "<#" + ColorUtility.ToHtmlStringRGB(color) + ">" + name + "</color>";
        }

    }

}