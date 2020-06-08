using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class WelcomeMessage : Message
    {
        public override MessageType Type => MessageType.Welcome;

        public int PlayerID { get; set; }
        public uint Color { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteInt(PlayerID);
            writer.WriteUInt(Color);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            PlayerID = reader.ReadInt();
            Color = reader.ReadUInt();
        }
    }
}
