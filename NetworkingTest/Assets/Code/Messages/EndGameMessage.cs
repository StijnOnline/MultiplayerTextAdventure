using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class EndGameMessage : Message
    {
        public override MessageType Type => MessageType.EndGame;
        //stuff
    }
}
