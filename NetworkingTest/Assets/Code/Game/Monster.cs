using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Game {
    public class Monster {

        const ushort DEFAULT_HEALTH = 5;
        const ushort DEFAULT_DAMAGE = 2;

        public ushort health;
        public ushort damage;

        public Monster(ushort health = DEFAULT_HEALTH, ushort damage = DEFAULT_DAMAGE) {
            this.health = health;
            this.damage = damage;
        }

    }
}
