using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class StartGameMessage : Message
    {
        public StartGameMessage(ushort startHealth) {
            this.startHealth = startHealth;
        }
        public override MessageType Type => MessageType.StartGame;

        public ushort startHealth { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteUShort(startHealth);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            startHealth = reader.ReadUShort();
        }
    }
}
