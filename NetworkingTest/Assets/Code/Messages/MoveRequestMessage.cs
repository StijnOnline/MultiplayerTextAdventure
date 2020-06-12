using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class MoveRequestMessage : Message
    {
        public override MessageType Type => MessageType.MoveRequest;

        public Server.Game.Directions direction { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteByte((byte)direction);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            direction = (Server.Game.Directions)reader.ReadByte();
        }
    }
}
