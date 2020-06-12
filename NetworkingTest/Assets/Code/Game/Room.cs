using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Game {
    public class Room {
        public bool exit = false;
        public Monster monster = null;
        public int treasure = 0;
        public List<int> playerIdsInRoom = new List<int>();
    }
}
