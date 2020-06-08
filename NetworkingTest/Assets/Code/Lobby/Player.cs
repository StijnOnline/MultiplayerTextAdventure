using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    public class Player {
        public string name { get; set; } = null;
        public Color color {get; private set; }

        public Player(uint color) {
            this.color.FromUInt(color);
        }

        public Player(Color color) {
            this.color = color;
        }

    }

}