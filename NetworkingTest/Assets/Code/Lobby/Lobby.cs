using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code {
    public class Lobby {

        public int you { get; private set; }
        public Dictionary<int, Player> players = new Dictionary<int, Player>(); //should i make it private and create access functions?
        
        /// <summary>
        /// Adds a player and set 'you' var
        /// </summary>
        public void SetPlayer(int playerID, Player newPlayer) {
            players.Add(playerID, newPlayer);
            you = playerID;
        }

    }
}
