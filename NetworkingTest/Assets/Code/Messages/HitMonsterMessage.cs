using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class HitMonserMessage : Message
    {
        public override MessageType Type => MessageType.HitMonster;


        public int PlayerID { get; set; }
        public ushort damage { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteUShort(damage);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            damage = reader.ReadUShort();
        }
    }
}
