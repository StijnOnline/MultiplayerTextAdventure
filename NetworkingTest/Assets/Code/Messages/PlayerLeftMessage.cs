using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    /// <summary>
    /// Mainly used as a message to keep a connection alive 
    /// </summary>
    public class PlayerLeftMessage : Message
    {
        public int PlayerID { get; set; }

        public override MessageType Type => MessageType.PlayerLeft;

        public override void SerializeObject(ref DataStreamWriter writer) {
            base.SerializeObject(ref writer);

            writer.WriteInt(PlayerID);
        }

        public override void DeserializeObject(ref DataStreamReader reader) {
            base.DeserializeObject(ref reader);

            PlayerID = reader.ReadInt();
        }
    }
}
